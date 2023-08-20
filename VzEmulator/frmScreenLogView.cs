using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VzEmulator.Peripherals;

namespace VzEmulator
{
    public partial class frmScreenLogView : Form
    {
        internal IMemoryMonitor ScreenOutput { get; set; }
        public frmScreenLogView()
        {
            InitializeComponent();
        }

        private void frmPrinterView_Load(object sender, EventArgs e)
        {
            ScreenOutput.MemoryChanged += ScreenOutput_MemoryChanged;
        }

        int LastAddress;
        byte LastValue;
        bool clearing;
        int clearingCount = 0;
        private void ScreenOutput_MemoryChanged(object sender, AddressChangedEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                var address =  e.Address- VzConstants.VideoRamStart;

                var c = e.NewData;
                if ((c & 0x60) == 0x60) c -= 0x40; //character set conversion
                if ((c & 0x1F) == c) c += 0x040; //character set conversion

                if (address == LastAddress && c == LastValue) return;
                if (address == LastAddress + 1 && c == 0x20) return; //ignore space after cursor movement

                //insert crlf if last address was end of line and this address is start of line. The lines are 32 chars long
                if (address % 32 == 0 && LastAddress % 32 > 0 && address>LastAddress || address % 32 == 0 && LastAddress == address - 32 || LastAddress!= 32*15 && address == 32*15) {
                    if (LastAddress < 510) //1 less than the last char on the screen, since the last char is filtered out by the "ignore space after cursor movement" check above
                        textBox1.Text += Environment.NewLine;
                }
                if (address!= 480 && address % 32 == 0 && LastAddress % 32 > 0 && address < LastAddress && LastAddress-address<32) return; //ignore character on first column of line. cursor moves there before going down a line
                if (LastAddress >= 32 * 15 && address == 0) // moved from last row of screen to first position. Likely a scroll and screen refresh. Ignore all further updates until the last line again. TODO special case for clear screen
                {
                    clearing = true;   
                }
                if (clearing)
                {
                    if (address < 32 * 16-1 && clearingCount<512)
                    {
                        clearingCount++;
                        return;
                    }
                    clearingCount = 0;
                    clearing = false;
                }
                if (address==480 && LastAddress>480)
                {
                    LastAddress = address;
                    LastValue = c;
                    return;
                }

                LastAddress = address;
                LastValue = c;

                textBox1.Text += (char)c; //todo filter out cursor movement
            }));
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }
    }
}
