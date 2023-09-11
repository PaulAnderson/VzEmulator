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
        ActiveDriveChanged,
    }

    internal class DriveStatusChange
    {
        public DriveStatusChangeType ChangeType { get; set; }
        public int DriveNumber { get; set; }
        public int CurrentTrack { get; set; }
        public int CurrentSector { get; set; }
        public byte[] bytesWritten { get; set; }
        public byte[] bytesRead { get; set; }
        public int DataLocation { get; set; }
    }
}
