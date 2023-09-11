using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VzEmulator.Peripherals;

namespace VzEmulator
{
    internal class StatusUpdater : IDriveWatcher
    {
        private readonly ToolStripItem _controlToUpdate;

        public StatusUpdater(ToolStripItem controlToUpdate)
        {
            _controlToUpdate = controlToUpdate;
        }

        public void NotifyDriveStatusChange(DriveStatusChange changeData)
        {
            switch (changeData.ChangeType)
            {
                case DriveStatusChangeType.ActiveDriveChanged:
                    _controlToUpdate.Text = string.Format("Changed Active Drive: {0}", changeData.DriveNumber + 1);
                    break;
                case DriveStatusChangeType.DataWrite:
                    _controlToUpdate.Text = string.Format($"Disk Write: Drive {changeData.DriveNumber+1} Track {changeData.CurrentTrack}, Sector {changeData.CurrentSector}, {changeData.bytesWritten.Length} bytes written. You must save disk image to persist changes");
                    break;
                 
            }
        }

    }
}
