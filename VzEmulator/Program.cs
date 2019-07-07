using System;
using System.Windows.Forms;

namespace VzEmulator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new frmMain();
            var presenter = new MachinePresenter(new Machine(), form);
            form.Presenter = presenter;
            Application.Run(form);
        }
    }
}
