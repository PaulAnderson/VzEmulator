using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Peripherals
{
    public class Drive : IPeripheral
    {
        const ushort DosInitRoutineAddress = 0x4b08;
        byte lastVal = 0;
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
        Disk diskContent = new Disk();

        public Tuple<ushort, ushort> PortRange => new Tuple<ushort, ushort>(0x10, 0x1f);

        public Tuple<ushort, ushort> MemoryRange => new Tuple<ushort, ushort>(0x4b08, 0x4b08);

        private bool _debugEnabled = false ;
        public bool DebugEnabled { get => _debugEnabled; set => _debugEnabled = value; }

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

                int fileIndex = (diskCurrentTrack * Disk.TrackLength) + (diskLocationOntrack / 8);

                //bounds check fileIndex
                if (fileIndex < 0) fileIndex = 0;
                if (fileIndex > diskContent.Length - 1) fileIndex = diskContent.Length - 1;

                value = diskContent[fileIndex]; //Dos routine reads port 11 once for each change to bit 7 of port 12. 

                if (!prevFileIndex.HasValue || prevFileIndex != fileIndex)
                {
                    if (!prevFileIndex.HasValue || fileIndex - prevFileIndex > 1 || diskReadsDebugged >= 15)
                    {
                        if (diskReadsDebugged >= 15)
                        {
                            diskReadsDebugged = 0;
                                if (_debugEnabled) Console.WriteLine();
                        }
                        if (_debugEnabled) Console.Write("Disk Read : {0:X4}: {1:X2} ", fileIndex, value);
                    }
                    else
                    {
                        if (_debugEnabled) Console.Write("{0:X2} ", value);
                        diskReadsDebugged++;
                    }
                }
                prevFileIndex = fileIndex;
                return value;
            }
            if (address == 0x12)
            {
                if (lastVal == 0)
                {
                    value = 0x80;
                    lastVal = value;

                    if (!prevWriteEnable)
                    {
                        //update position unless in write mode (write function takes care of its own location)
                        diskLocationOntrack++;
                        if (diskLocationOntrack > (Disk.TrackLength * 8) + 10)
                            diskLocationOntrack = 0;
                    }
                }
                else
                {
                    value = 0x00;
                    lastVal = value;

                }
                return value;
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
                byte stepNo = (byte)(value & 0xF);
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
                        //all 8 bits received, write to file
                        int fileIndex = (diskCurrentTrack * Disk.TrackLength) + (diskLocationOntrack / 8);

                        //bounds check fileIndex
                        if (fileIndex < 0) fileIndex = 0;
                        if (fileIndex > diskContent.Length - 1) fileIndex = diskContent.Length - 1;

                        //write byte
                        diskContent[fileIndex] = currentWritingByte;

                        if (_debugEnabled)
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
                if (diskCurrentTrack > 40) diskCurrentTrack = 40;
                if (_debugEnabled) Console.WriteLine("Disk Track: {0}", diskCurrentTrack);

            }
            if ((stepNo == 8 && lastStepNo == 9 && lastStepNo2 == 1) |
                    (stepNo == 2 && lastStepNo == 6 && lastStepNo2 == 4))
            {
                diskCurrentTrack -= 1;
                if (diskCurrentTrack < 0) diskCurrentTrack = 0;
                if (_debugEnabled) Console.WriteLine("Disk Track: {0}", diskCurrentTrack);

            }
            lastStepNo2 = lastStepNo;
            lastStepNo = stepNo;
            //end stepper motor logic
        }

        public void LoadDiskImage(string fileName)
        {
            diskContent.LoadDiskImage(fileName);
        }
        public void SaveDiskImage(string fileName)
        {
            diskContent.SaveDiskImage(fileName);
        }
    }
}
