using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    enum DriveStatusChangeType
    {
        Control,
        DataWrite,
        DataRead,
        WriteProtectRead,
    }

    internal class DriveStatusChange
    {
        public DriveStatusChangeType ChangeType { get; set; }
        public int DriveNumber { get; set; }
        public int CurrentTrack { get; set; }
        public byte byteWritten { get; set; }
        public byte byteRead { get; set; }
        public int DataLocation { get; set; }
    }
}
