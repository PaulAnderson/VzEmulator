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

namespace VzEmulator.Debugger
{
    public partial class frmDriveWatcher : Form , IDriveWatcher
    {
        public frmDriveWatcher()
        {
            InitializeComponent();
        }

        void IDriveWatcher.NotifyDriveStatusChange(DriveStatusChange changeData)
        {
            if (changeData.ChangeType == DriveStatusChangeType.Control)
                this.Invoke(new MethodInvoker(delegate ()
                {
                    TrackLabel.Text = changeData.CurrentTrack.ToString();
                }));
            
        }
    }
}
