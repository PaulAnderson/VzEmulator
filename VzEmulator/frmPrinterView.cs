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
            textBox1.Text = GetPrintBufferContentsAll();
        }

        private void frmPrinterView_FormClosing(object sender, FormClosingEventArgs e)
        {
            PrinterOutput.LinePrinted -= PrinterOutput_LinePrinted;

        }
        private void PrinterOutput_LinePrinted(object sender, EventArgs e)
        {
            if (this.IsDisposed) return; 

            this.Invoke(new MethodInvoker(delegate ()
            {
                textBox1.Text += GetPrintBufferContentsLast();
            }));
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ClearPrintBuffer();
            textBox1.Clear();
        }

        private string GetPrintBufferContentsAll()
        {
            var sb = new StringBuilder(PrinterOutput.PrintedLines.Count * 64);
            foreach (string line in PrinterOutput.PrintedLines)
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        private string GetPrintBufferContentsLast()
        {
            return PrinterOutput.PrintedLines.Last() + "\r\n";
        }

        private void ClearPrintBuffer()
        {
            PrinterOutput.PrintedLines.Clear();
        }
    }
}
