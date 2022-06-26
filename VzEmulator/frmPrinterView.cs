using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VzEmulator.Peripherals;

namespace VzEmulator
{
    public partial class frmPrinterView : Form
    {
        internal IPrinterOutput PrinterOutput { get; set; }
        public frmPrinterView()
        {
            InitializeComponent();
        }

        private void frmPrinterView_Load(object sender, EventArgs e)
        {
            PrinterOutput.LinePrinted += PrinterOutput_LinePrinted;
            var sb = new StringBuilder(PrinterOutput.PrintedLines.Count * 64);
            foreach (string line in PrinterOutput.PrintedLines)
            {
                sb.AppendLine(line);
            }
            textBox1.Text = sb.ToString();
        }

        private void PrinterOutput_LinePrinted(object sender, EventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                textBox1.Text += PrinterOutput.PrintedLines.Last() + "\r\n";
            }));
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            PrinterOutput.PrintedLines.Clear();
            textBox1.Clear();
        }
    }
}
