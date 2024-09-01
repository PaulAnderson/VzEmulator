using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using VzEmulator.Screen;

namespace VzEmulator
{
    public partial class frmMemoryView : Form
    {
        const int RowLength = 8;
        const int Lines = 16;

        Byte[] Memory = new byte[2000];
        HashSet<int> SearchResults = new HashSet<int>();
        HashSet<int> SearchResultsForRender = new HashSet<int>();

        IMemoryAccessor EditingMemory;

        Timer RefreshTimer;
        Timer CursorTimer;

        int startAddr;
        bool useLightColour = true;

        int cursorLocation = 0;
        int cursorNibbleLocation = 0;
        byte cursorNibbleValue = 0;
        bool cursorBlink;

        bool wrapAround=true;
        int maxAddress = 0x10000;

        private TextboxValidation validation = new TextboxValidation();
        private bool IsFormClosing;
        private const int  screenXwidth = 48;

        public frmMemoryView(IMemoryAccessor memory, string Title = null)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Title))
            {
                this.Text = Title;
            }

            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);

            var graphicsPainter = new FormsGraphicsPainter(pictureBox1, Memory, 5, screenXwidth, 16)
            {
                UseFixedScale = true,
            };
            graphicsPainter.Annotator = new SearchResultsAnnotator() { SearchResults = SearchResultsForRender };

            this.EditingMemory = memory;
            if (memory.Size>maxAddress)
            {
                //disk edit
                maxAddress = memory.Size;
                wrapAround = false;
                txtStartAddress.Text = "0x0000";
            }    

            ReadAddrFromTextBox();

            RefreshTimer = new Timer();
            RefreshTimer.Interval = 10; //ms
            RefreshTimer.Tick += (v, a) => RefreshScreen();
            RefreshTimer.Start();
            CursorTimer = new Timer();
            CursorTimer.Interval = 500; //ms
            CursorTimer.Tick += (v, a) => cursorBlink = !cursorBlink;
            CursorTimer.Start();
 

            ClearScreen();
        }

        private void pictureBox1_MouseWheel(object sender,  MouseEventArgs e)
        {
            Scroll(e.Delta);
        }
        private new void Scroll(int mouseWheelDelta)
        {
            if (mouseWheelDelta != 0)
            {
                var delta = mouseWheelDelta / RowLength;
                delta = Math.Abs(delta);
                delta >>= 3;
                if (delta == 0) delta = 1;
                delta <<= 3;
                if (mouseWheelDelta < 0) delta = 0 - delta;
                startAddr = (ushort)(startAddr - delta);
            }
            txtStartAddress.Text = string.Format("0X{0:X4}", startAddr);
        }
        private void ClearScreen()
        {
            for (var i=0;i<Memory.Length;i++)
            {
                Memory[i] = (byte)' ';
                Memory[i] += ColourOffset(); 
            }
        }
        private byte ColourOffset()
        {
            return useLightColour ? VzConstants.LightColorCharOffset : (byte)0;
        }
        private void RefreshScreen()
        {
            var blockAddr = startAddr;

            SearchResultsForRender.Clear();

            for (var line = 0; line < Lines; line++ )
            {
                var startLineAddr = line * screenXwidth;
                var blockAddrChars = 4;
                if (blockAddr > 0xFFFF) blockAddrChars = 5;
                var blockAddrStr = string.Format($"{{0:X{blockAddrChars}}}", blockAddr);

                WriteString(Memory, startLineAddr, "     ", true);
                WriteString(Memory, startLineAddr, blockAddrStr,true);

                for (var i = 0; i < RowLength; i++ )
                {
                    var isCursorLocation = cursorLocation == i + line * RowLength;
                    var value = EditingMemory[blockAddr + i];
                    var valueStr = string.Format("{0:X2}", value);
                    var colour = !(isCursorLocation & cursorBlink & (this.ActiveControl==null));
                    var hexValuesStartX = 6;
                    var charValuesStartX = 32;
                    var hexCharLocationStart = startLineAddr + hexValuesStartX + i * 3;
                    var CharacterLocation = startLineAddr + charValuesStartX + i;

                    WriteString(Memory, hexCharLocationStart, valueStr, colour); //Write hex value and space
                    Memory[CharacterLocation] = ConvertChar(value,colour); //Write char value

                    //Use annotator to highlight search results. This block converts the memory address to screen location.
                    if (SearchResults.Contains(blockAddr + i))
                    {
                        SearchResultsForRender.Add(hexCharLocationStart);
                        SearchResultsForRender.Add(hexCharLocationStart+1);
                        SearchResultsForRender.Add(CharacterLocation);
                    }

                    if (isCursorLocation && cursorNibbleLocation > 0)
                    {
                        //Show edit to high nibble
                        WriteString(Memory, startLineAddr + hexValuesStartX + i * 3, string.Format("{0:X1}",cursorNibbleValue>>4), true);
                    }

                    Memory[startLineAddr + hexValuesStartX + i * 3 + 2] = 0x60;
                    if (value == 20) Memory[startLineAddr + hexValuesStartX + i * 3 + 2] = ((byte)'*')+0x40;
                }
                blockAddr += (ushort)RowLength;
            }
        }

        private void WriteString(Byte[] Dest,int Start,string StringToWrite,bool colour)
        {
            for (var i =0; i < StringToWrite.Length; i++)
            {
                char c = Convert.ToChar(StringToWrite.Substring(i, 1));
                Dest[Start + i] = (byte)ConvertChar(c,colour);
            }
        }
        private char ConvertChar(char value, bool colour)
        {
            if (colour)
            {
                if (value < 0x40) value += (char)0x40;
            } else
            {
                if (value > 0x40) value -= (char)0x40;
            }
            return value;
        }
        private byte ConvertChar(byte value, bool colour)
        {
            if (colour)
            {
                if (value < 0x40) value += 0x40;
            }
            else
            {
                if (value > 0x40) value -= 0x40;
            }
            return value;
        }
        private void txtStartAddress_Validating(object sender, CancelEventArgs e)
        {
            if (IsFormClosing) return;

            validation.ValueTextBox_Validate(sender, e);
        }

        private void txtStartAddress_Leave(object sender, EventArgs e)
        {
            ReadAddrFromTextBox();
        }
        private void ReadAddrFromTextBox()
        {
            try
            {
                startAddr = MemUtils.StringToUShort(txtStartAddress.Text);
            }
            catch (Exception )
            {
                //do nothing
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            bool addSearch = (SearchResults.Count == 0);
            var searchTerm = MemUtils.StringToByte(txtSearch.Text);
            if (addSearch)
                for (var i=0;i< EditingMemory.Size;i++)
                {
                    if (EditingMemory[i] == searchTerm)
                        SearchResults.Add(i);
                }
            else
                for (var i = 0; i < EditingMemory.Size; i++)
                {
                    if (EditingMemory[i] != searchTerm && SearchResults.Contains(i))
                        SearchResults.Remove(i);
                }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            SearchResults.Clear();
        }

        private void frmMemoryView_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.ActiveControl != null)
            {
                //Search controls
                if (e.KeyData == Keys.Tab)
                    this.ActiveControl = null; //set focus to main editor

            } else { 
                //Main editor

                bool cursorMoved = false;
                switch (e.KeyData)
                {
                    case Keys.Home:
                        startAddr = 0;
                        cursorMoved = true;
                        break;
                    case Keys.PageUp:
                        startAddr -= RowLength*Lines;
                        cursorMoved = true;
                        break;
                    case Keys.PageDown:
                        startAddr += RowLength * Lines;
                        cursorMoved = true;
                        break;
                    case Keys.Up:
                        cursorLocation -= RowLength;
                        cursorMoved = true;
                        break;
                    case Keys.Down:
                        cursorLocation += RowLength;
                         cursorMoved = true;
                        break;
                    case Keys.Left:
                        cursorMoved = true;
                        if (cursorNibbleLocation==0)
                            cursorLocation--;
                        break;
                    case Keys.Right:
                        cursorLocation++;
                        cursorMoved = true;
                        break;
                    case Keys.End:
                        startAddr = (EditingMemory.Size - (RowLength * Lines));
                        cursorMoved = true;
                        break;
                    default:
                        try
                        {
                            var key = ((char)e.KeyCode).ToString();
                            var value = Byte.Parse(key, System.Globalization.NumberStyles.HexNumber);
                            if (cursorNibbleLocation == 0)
                            {
                                cursorNibbleValue = (byte)((value & 0x0F) << 4);
                                cursorNibbleLocation = 1;
                            } else
                            {
                                var addr = startAddr + cursorLocation;
                                if (wrapAround && addr >= maxAddress) addr -= maxAddress;
                                Console.WriteLine($"Writing {value:X2} to {(addr):X4}");
                                EditingMemory[addr] = (byte)(cursorNibbleValue | (byte)(value & 0x0F));
                                cursorLocation++;
                                cursorMoved = true;

                            }
                        }
                        catch (Exception)
                        {
                            //do nothing
                        }
                        break;
                }
                if (cursorMoved)
                {
                    //Clear nibble editing
                    cursorNibbleLocation = 0;
                    cursorNibbleValue = 0;

                    //Validate cursor position, scroll if off edge
                    if (cursorLocation >= RowLength * Lines)
                    {
                        cursorLocation -= RowLength;
                        startAddr += RowLength;
                    }
                    if (cursorLocation < 0)
                    {
                        cursorLocation += RowLength;
                        startAddr -= RowLength;
                    }

                }

                //validate memory postion. Wrap around if out of range
                if (wrapAround)
                {
                    if (startAddr < 0)
                    {
                        //use startAddr to store the offset from the end of memory, to position the cursor in the expected place
                        var cursorOffset = startAddr;
                        if (cursorOffset > 0) cursorOffset = 0;

                        startAddr = maxAddress - RowLength * Lines;

                        //set cursor position based on earlier calculated offset from end of memory
                        cursorLocation = RowLength*Lines + cursorOffset;
                    }
                    if (startAddr > maxAddress - RowLength * Lines)
                    {
                        startAddr = 0;
                        cursorLocation = 0;
                    }
                } else
                {
                    //start memory view at 0, set cursor to location 0
                    if (startAddr < 0) startAddr = 0;
                    if (startAddr > maxAddress - RowLength * Lines) startAddr = maxAddress - RowLength * Lines;
                }
            }

        }

        private void frmMemoryView_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in pnlTop.Controls)
            {
                ctrl.PreviewKeyDown += Button_PreviewKeyDown;
            }

            this.ActiveControl = null;
        }

        private void Button_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void frmMemoryView_ResizeEnd(object sender, EventArgs e)
        {
            //Text = $"Memory View - {EditingMemory.ToString()} - {EditingMemory.Size} bytes. Size: {this.Size.Width}x{this.Size.Height}";
        }

        private void frmMemoryView_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsFormClosing = true;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }


    }
}
