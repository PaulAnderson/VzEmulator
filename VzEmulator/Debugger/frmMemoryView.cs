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

        public frmMemoryView(IMemoryAccessor memory)
        {
            InitializeComponent();

            pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);

            var graphicsPainter = new GraphicsPainter(pictureBox1, Memory, null, 0, 5)
            {
                UseFixedScale = true,
            };

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


            for (var line = 0; line < Lines; line++ )
            {
                var startLineAddr = line << 5;
                var blockAddrStr = string.Format("{0:X4}", blockAddr);

                WriteString(Memory, startLineAddr, blockAddrStr,true);

                for (var i = 0; i < RowLength; i++ )
                {
                    var isCursorLocation = cursorLocation == i + line * RowLength;
                    var value = EditingMemory[blockAddr + i];
                    var valueStr = string.Format("{0:X2}", value);
                    var colour = !(SearchResults.Contains(blockAddr + i)) && !(isCursorLocation & cursorBlink);

                    WriteString(Memory, startLineAddr + 5 + i * 3, valueStr, colour);

                    if (isCursorLocation && cursorNibbleLocation > 0)
                    {
                        //Show edit to high nibble
                        WriteString(Memory, startLineAddr + 5 + i * 3, string.Format("{0:X1}",cursorNibbleValue>>4), true);
                    }

                    Memory[startLineAddr + 5 + i * 3 + 2] = 0x60;
                    if (value == 20) Memory[startLineAddr + 5 + i * 3 + 2] = ((byte)'*')+0x40;
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
        private void txtStartAddress_Validating(object sender, CancelEventArgs e)
        {
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

        private void frmMemoryView_KeyDown(object sender, KeyEventArgs e)
        { 
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
                if (startAddr < 0) startAddr += maxAddress - 1; ;
                if (startAddr >= maxAddress) startAddr -= (maxAddress - 1);
            } else
            {
                if (startAddr < 0) startAddr = 0;
                if (startAddr >= maxAddress) startAddr = maxAddress - 1;
            }
        }

        private void frmMemoryView_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in pnlTop.Controls)
            {
                ctrl.PreviewKeyDown += Button_PreviewKeyDown;
            }
        }

        private void Button_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }
    }
}
