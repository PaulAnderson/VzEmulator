using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VzEmulator.Peripherals;

namespace VzEmulator
{
    interface IDriveWatcher
    {
        void NotifyDriveStatusChange(DriveStatusChange changeData);
    }
}
