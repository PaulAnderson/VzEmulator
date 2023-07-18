using System;

namespace VzEmulator.Peripherals
{
    public class Drive : IPeripheral , IClockSynced
    {
        const ushort DosInitRoutineAddress = 0x4b08;
        byte lastVal;
        int diskCurrentTrack = 5;
        int diskLocationOntrack = 0;
        int lastStepNo;
        int lastStepNo2;
        int? prevFileIndex = null;
        int diskReadsDebugged = 0;
        bool prevWriteEnable = false;
        byte writeBitNo = 0;
        byte firstHalfBit = 0;
        byte currentWritingByte = 0;
        int bytesWritten = 0;
        bool inDosInitRoutine = false;
        bool writeProtect = false;
        Disk[] diskContent = { new Disk(), new Disk() };
        public Disk Disk0 => diskContent[0];
        public Disk Disk1 => diskContent[1];

        byte activeDrive;

        public Tuple<ushort, ushort> PortRange { get; set; }

        public Tuple<ushort, ushort> MemoryRange { get; set; }

        public bool DebugEnabled { get; set; } = false;

        //timing data. Currently for debugging only
        decimal IClockSynced.ClockFrequency { get; set; }
        bool IClockSynced.ClockSyncEnabled { get; set; }
        int currentClockCycles; //Roll over every 3,5400,000 cycles
        int lastRead12ClockCycles;
        public Drive()
        {
            PortRange = new Tuple<ushort, ushort>(0x10, 0x1f);
            MemoryRange = new Tuple<ushort, ushort>(DosInitRoutineAddress, DosInitRoutineAddress);
        }

        public byte? HandleMemoryRead(ushort address)
        {
            //intercept dos init keyword, used to reset to start of file to align sectors
            if (address == DosInitRoutineAddress )
            {
                inDosInitRoutine = true;
            }
            return null;
        }

        public bool HandleMemoryWrite(ushort address, byte value)
        {
            return false;
        }

        public byte? HandlePortRead(ushort address)
        {
            byte value = 0;

            if (address == 0x11)
            {
                inDosInitRoutine = false;

                int fileIndex = (diskCurrentTrack * diskContent[activeDrive].TrackLength) + (diskLocationOntrack / 8);

                //bounds check fileIndex
                if (fileIndex < 0) fileIndex = 0;
                if (fileIndex > diskContent[activeDrive].Length - 1) fileIndex = diskContent[activeDrive].Length - 1;

                //a more accurate emulation would shift the value in one bit at a time but this is enough for now.
                value = diskContent[activeDrive][fileIndex]; //Dos routine reads port 11 once for each change to bit 7 of port 12. 

                if (!prevFileIndex.HasValue || prevFileIndex != fileIndex)
                {
                    if (!prevFileIndex.HasValue || fileIndex - prevFileIndex > 1 || diskReadsDebugged >= 15)
                    {
                        if (diskReadsDebugged >= 15)
                        {
                            diskReadsDebugged = 0;
                                if (DebugEnabled) Console.WriteLine();
                        }
                        if (DebugEnabled) Console.Write("Disk Read : {0:X4}: {1:X2} ", fileIndex, value);
                    }
                    else
                    {
                        if (DebugEnabled) Console.Write("{0:X2} ", value);
                        diskReadsDebugged++;
                    }
                }
                prevFileIndex = fileIndex;
#if DEBUG
    //            Console.WriteLine($"{currentClockCycles}: Disk Read 0x11 Value {value:X2}");
#endif
                return value;
            }
            if (address == 0x12)
            {
                value = lastVal;
                if (currentClockCycles- lastRead12ClockCycles>35 | currentClockCycles - lastRead12ClockCycles<0) //invert bit every 10us
                {
                    lastRead12ClockCycles = currentClockCycles;

                    value ^= 0x80;
                    lastVal = value;
                    if (value==0x80)
                    {
                        if (!prevWriteEnable)
                        {
                            //update position unless in write mode (write function takes care of its own location)
                            diskLocationOntrack++;
                            if (diskLocationOntrack >= (diskContent[activeDrive].TrackLength * 8))
                                diskLocationOntrack = 0;
                        }
                    }
                }
#if DEBUG
      //          Console.WriteLine($"{currentClockCycles}: Disk Read 0x12 Value {value:X2}");
#endif 
                return value;
            }
            if (address==0x13)
            {
                if (writeProtect)
                    return 0x80; 
                else
                    return 0x00;
            }

            return null;
        }

        public void HandlePortWrite(ushort address, byte value)
        {
            if (address == 0x10)
            {
                //Disk drive control
                /*
                Bit 0 - 3 : stepper motor
                Bit 4 : Drive 1 Active (1=active)
                Bit 5 : Write Data
                Bit 6 : Write Request. Enable? (0=Active)
                Bit 7 : Drive 2 Active (1=active)
                 */

                //todo Drive selection. Record current value in latch.
                //Handle drive 1 and drive 2 active
                if ((value & 0x10) == 0x10) activeDrive = 0;
                else if ((value & 0x80) == 0x80) activeDrive = 1;

                byte stepNo = (byte)(value & 0x0F);
                Step(stepNo);

                byte writeControl = (byte)(value & 0xF0);
                Write(writeControl);
                
            }
        }

        private void Write(byte value)
        {
            /*
                Bit 4 : Drive 1 Active(1 = active)
                Bit 5 : Write Data
                Bit 6 : Write Request. Enable ? (0 = Active)
                Bit 7 : Drive 2 Active(1 = active)
            */
            bool isWriteEnable = !((value & 0x40) == 0x40);

            if (isWriteEnable)
            {
                if (prevWriteEnable)
                {
                    if ((writeBitNo & 1) == 0)
                    {
                        //first written bit of actual bit. Just store it for later comparison
                        firstHalfBit = (byte)((value & 0x20) >> 5);
                    }
                    else
                    {
                        byte thisHalfBit = (byte)((value & 0x20) >> 5);
                        byte xorResult = (byte)(firstHalfBit ^ thisHalfBit);
                        byte shiftedResult = (byte)(xorResult << 7 - (writeBitNo / 2));
                        currentWritingByte |= shiftedResult;
                    }
                    if (writeBitNo == 15)
                    {
                        //all 8 bits and associated clocks received, write to array
                        int fileIndex = (diskCurrentTrack * diskContent[activeDrive].TrackLength) + (diskLocationOntrack / 8);

#if DEBUG
                        //write to console current track, location on track and file index, and value
                        //Console.WriteLine("Disk write: Track: {0:X2} Location: {1:X4} FileIndex: {2:X4} Value: {3:X2}", diskCurrentTrack, diskLocationOntrack/8, fileIndex, currentWritingByte);
#endif

                        //bounds check fileIndex
                        if (fileIndex < 0) fileIndex = 0;
                        if (fileIndex > diskContent[activeDrive].Length - 1)
                        {
                            
#if DEBUG
                            Console.WriteLine("Disk write out of bounds: {0:X4}", fileIndex);
#endif
                            fileIndex = diskContent[activeDrive].Length - 1;
                        }

                        //write byte
                        diskContent[activeDrive][fileIndex] = currentWritingByte;

                        if (DebugEnabled)
                        {
                            if ((bytesWritten & 0x0F) == 0x0)
                            {
                                Console.WriteLine();
                                Console.Write("Disk Write: {0:X4}: ", fileIndex);
                            }
                            Console.Write("{0:X2} ", currentWritingByte);
                        }

                        diskLocationOntrack += 8;
                        bytesWritten++;
                        writeBitNo = 0; //start next byte
                        currentWritingByte = 0;
                        firstHalfBit = 0;

                        //track wraps around
                        if (diskLocationOntrack >= (diskContent[activeDrive].TrackLength * 8))
                            diskLocationOntrack = 0;

                    }
                    else
                    {
                        writeBitNo++;
                    }
                }
                else
                {
                    //ignore first bit written, just result byte to be written
                    currentWritingByte = 0;
                    writeBitNo = 0;
                    firstHalfBit = 0;

                    //Adjust write location to next full byte
                    if (inDosInitRoutine)
                    {
                        diskLocationOntrack = 0;
                    }
                    else
                    {
                        int writeOffset = 2; //How many bytes skipped in the time to switch from reading to writing
                        diskLocationOntrack = ((int)(Math.Ceiling(diskLocationOntrack / 8.0) + writeOffset) * 8);
                    }

                    //Reset write counter
                    bytesWritten = 0;
                }
            }
            else
            {
                if (prevWriteEnable) Console.WriteLine();
            }
            prevWriteEnable = isWriteEnable;

        }

        private void Step(byte stepNo)
        {
            if ((stepNo == 1 && lastStepNo == 9 && lastStepNo2 == 8)
                                | (stepNo == 4 && lastStepNo == 6 && lastStepNo2 == 2))
            {
                diskCurrentTrack += 1;
                if (diskCurrentTrack > (diskContent[activeDrive].Tracks-1)) diskCurrentTrack = diskContent[activeDrive].Tracks - 1;
                OnTrackUpdated();
            }
            if ((stepNo == 8 && lastStepNo == 9 && lastStepNo2 == 1) |
                    (stepNo == 2 && lastStepNo == 6 && lastStepNo2 == 4))
            {
                diskCurrentTrack -= 1;
                if (diskCurrentTrack < 0) diskCurrentTrack = 0;
                OnTrackUpdated();
            }
            lastStepNo2 = lastStepNo;
            lastStepNo = stepNo;
        }

        public void LoadDiskImage(string fileName)
        {
            diskContent[activeDrive].LoadDiskImage(fileName);
        }
        public void SaveDiskImage(string fileName, bool reFormat = false)
        {
            diskContent[activeDrive].ReformatAndSaveDiskImage(fileName);
        }

        protected void OnTrackUpdated()
        {
            diskLocationOntrack = 0; //Nice to keep the tracks aligned in the DSK file
            if (DebugEnabled) Console.WriteLine("Disk Track: {0}", diskCurrentTrack);

            OnDriveStatusChangeEvent(new DriveStatusChange
            {
                ChangeType = DriveStatusChangeType.Control,
                CurrentTrack = diskCurrentTrack
            });
        }
        internal event EventHandler<DriveStatusChange> DriveStatusChangeEvent;

        internal void OnDriveStatusChangeEvent(DriveStatusChange e)
        {
            DriveStatusChangeEvent?.Invoke(this, e);
        }

        public void Reset()
        {
            //do nothing
        }

        void IClockSynced.ProcessClockCycles(int periodLengthInCycles)
        {
            int maxClockCycles = (int)(((IClockSynced)this).ClockFrequency*1000000);
            currentClockCycles +=periodLengthInCycles;
            if (currentClockCycles > maxClockCycles)
            {
                currentClockCycles -= maxClockCycles;
            }
        }
    }
}
