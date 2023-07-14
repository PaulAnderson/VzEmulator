using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VzEmulator.Peripherals
{
    public class Disk : IMemoryAccessor
    {
        //Disk structure (per VZ300 Technical Manual)
        //40 Tracks of 16 Sectors.
        //Each sector has 25 bytes sector identification, 128 bytes of data, 2 bytes checksum (155 bytes total)
        //155x16 = 2480 bytes per track
        //2480x40 = 99200 bytes per disk

        //Sector format:
        //GAP1: 6 or 7 bytes of 0x80h, 1 byte of 0x00h (7 Bytes in tech manual, 6 bytes actually written by ROM)
        //IDAM: 0xFE,0xE7,0x18,0xC3,Track No, Sector No, checksum
        //GAP2: 5 bytes of 0x80h, 1 byte of 0x00h, 0xC3,0x18,0xE7,0xFE
        //Data: 128 Bytes
        //Checksum: 2 Bytes
        Byte[] standardSectorOrder = new Byte[] { 0, 11, 6, 1, 12, 7, 2, 13, 8, 3, 14, 9, 4, 15, 10, 5 };
        Byte[] GAP1IDAM = new Byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00, 0xFE, 0xE7, 0x18, 0xC3 };
        Byte[] GAP2 = new Byte[] { 0x80, 0x80, 0x80, 0x80, 0x80, 0x00, 0xC3, 0x18, 0xE7, 0xFE };

        public int Size = 99200;
        public int TrackLength = 2480;   // 99200/40
        public int Tracks = 40;
        public const int SectorsPerTrack = 16;
        private string fileName;
        private byte[] contents;
        public byte this[int address] {
            get
            {
                return contents[address];
            } 
            set
            {
                contents[address] = value;
                
            }
        }

        private void reCalculateChecksum(int indexOfDataChange)
        {
            //Called when changes are made to the disk contents using the disk editor
            //Todo make more efficient by only finding the current sector
            var sectors=FindSoftSectors(ignoreCheckSumErrors:true);
            var sector = sectors.Where(s => s.startIndex <= indexOfDataChange && s.startIndex+s.length >= indexOfDataChange).FirstOrDefault();
            if (sector.length > 0)
            {
                //todo Combine with code in WriteSectors, reuse checksum calc
                UpdateSectorDataChecksum(sector);

            }
        }
        private void UpdateSectorDataChecksum(Sector sector) { 
            var checksumValue = CalculateChecksum(sector.data,0,sector.dataLength-1);
            contents[GetSectorChecksumLocation(sector)] = (byte)(checksumValue & 0xFF);
            contents[GetSectorChecksumLocation(sector)+1] = (byte)((checksumValue >> 8) & 0xFF);
        }
        private ushort CalculateChecksum(byte[] data, int startAddress, int endAddress)
        {
            //caclulate checksum by adding all bytes mod 2^16
            ushort checksum = 0;
            for (int i = startAddress; i <= endAddress; i++)
            {
                checksum += data[i];
            }
            return checksum;

        }
        private int GetSectorChecksumLocation(Sector sector)
        {
            return sector.startIndex + GAP1IDAM.Length + 3 + GAP2.Length + sector.dataLength + 1;
        }
        public int Length { get
            {
                return contents.Length;
            }
        }

        int IMemoryAccessor.Size => contents.Length;

        byte IIndexer.this[int address] { get => contents[address]; set  { contents[address] = value; reCalculateChecksum(address); } }

        public Disk() : this(new List<Sector>())
        {
        }

        public Disk(IEnumerable<Sector> sectors)
        {
            contents = new Byte[Size];
            WriteSectors(sectors);
        }
        public void WriteSectors(IEnumerable<Sector> sectors) { 
            //Create disk image based on sectors

            int standardSectorLength = GAP1IDAM.Length + 3 + GAP2.Length + 128 + 2;
            for (int track = 0;track<Tracks;track++)
            {
                for (int sector = 0; sector<SectorsPerTrack;sector++)
                {
                    var mappedSectorNo = standardSectorOrder[sector];
                    var sectorData = sectors.Where(s => s.trackNo == track && s.sectorNo == mappedSectorNo).FirstOrDefault();
                    int sectorStartIndex = track * TrackLength + sector * standardSectorLength;
                    GAP1IDAM.CopyTo(contents, sectorStartIndex);
                    contents[sectorStartIndex + GAP1IDAM.Length] = (byte)track;
                    contents[sectorStartIndex + GAP1IDAM.Length + 1] = (byte)mappedSectorNo;
                    contents[sectorStartIndex + GAP1IDAM.Length + 2] = (byte)(track + mappedSectorNo);
                    GAP2.CopyTo(contents, sectorStartIndex + GAP1IDAM.Length + 3);
                    if (sectorData.dataLength>0)
                    {
                        Array.Copy(sectorData.data, 0, contents, sectorStartIndex + GAP1IDAM.Length + 3 + GAP2.Length, sectorData.dataLength);
                        //calculate checksum based on sectorData.data
                        int checksum = 0;
                        for (int i = 0; i < sectorData.dataLength; i++)
                        {
                            checksum += sectorData.data[i];
                        }
                        contents[sectorStartIndex + GAP1IDAM.Length + 3 + GAP2.Length + sectorData.dataLength] = (byte)(checksum & 0xFF);
                        contents[sectorStartIndex + GAP1IDAM.Length + 3 + GAP2.Length + sectorData.dataLength + 1] = (byte)((checksum >> 8) & 0xFF);
                    }
                }
            }
        }

        public void LoadDiskImage(string fileName)
        {
            var fileContents = File.ReadAllBytes(fileName);
            if (fileContents.Length>Size)
            {
                //todo handle unknown disk format
                contents = fileContents;

            }
            else
            {
                contents = new Byte[Size];
                fileContents.CopyTo(contents, 0);
            }
            this.fileName = fileName;

            //Check format
            var sectors = FindSoftSectors();

            //find first sector on track 1
            var firstSector = sectors.Where(s => s.trackNo == 1).OrderBy(s => s.sectorNo).First();
            TrackLength = firstSector.startIndex;

        }
        public void SaveDiskImage(string fileName )
        {
            File.WriteAllBytes(fileName, contents);
            this.fileName = fileName;
        }
        public void ReformatAndSaveDiskImage(string fileName)
        {
            var newDisk = new Disk(FindSoftSectors());
            newDisk.SaveDiskImage(fileName);
        }

        public struct Sector
        {
            public int trackNo;
            public int sectorNo;
            public int startIndex;
            public int dataStartIndex;
            public int dataLength;
            public int length;
            internal byte[] data;
        }
        public IEnumerable<Sector> FindSoftSectors(bool ignoreCheckSumErrors=false)
        {
            //search contents array for sector IDAMs
            //IDAM: 0xFE,0xE7,0x18,0xC3,Track No, Sector No, checksum
            //iterate over array and find sectors
            //if sector found, add to list
            
            List<Sector> sectors = new List<Sector>();

            //search for GAP1, IDAM and GAP2 in contents array
            for (int i=0;i<contents.Length;i++)
            {
                bool gap1Found = false;
                int gap1Lenth=14;//default
                if (!(contents[i] == 0x80)) continue;

                //find 5-7 bytes of 0xFE
                for (int j=4;j<=6;j++)
                {
                    if (contents[i + j] == 0x80 && contents[i + j+1] == 0x00 && contents[i + j+2] == 0xFE && contents[i + j+3] == 0xE7 && contents[i + j + 4] == 0x18 && contents[i + j + 5] == 0xC3)
                    {
                        gap1Found = true;
                        gap1Lenth = 9 + j;
                        continue;
                    }
                }
                if (gap1Found)
                {
                    //GAP1 found
                    //IDAM found
                    //check checksum
                    byte trackNo = contents[i + gap1Lenth - 3];
                    byte sectorNo = contents[i + gap1Lenth - 2];
                    byte checksum = contents[i + gap1Lenth - 1];

                    //calculate checksum
                    var calculatedChecksum = trackNo + sectorNo;
                    if (calculatedChecksum == checksum)
                    {
                        //checksum matches

                        //check/find GAP2
                        bool gap2Found = false;
                        for (int gap2Offset = gap1Lenth; gap2Offset < gap1Lenth+6; gap2Offset++)
                        {
                            if (contents[i + gap2Offset] == 0x80 &&
                                contents[i + gap2Offset + 1] == 0x80 &&
                                contents[i + gap2Offset + 2] == 0x80 &&
                                contents[i + gap2Offset + 3] == 0x80 &&
                                contents[i + gap2Offset + 4] == 0x80 &&
                                contents[i + gap2Offset + 5] == 0x00 &&
                                contents[i + gap2Offset + 6] == 0xC3 &&
                                contents[i + gap2Offset + 7] == 0x18 &&
                                contents[i + gap2Offset + 8] == 0xE7 &&
                                contents[i + gap2Offset + 9] == 0xFE)
                            {
                                //GAP2 found
                                //check data
                                //check checksum
                                gap2Found = true;
                                short expectedDataChecksum = 0;
                                for (int j = 0; j < 128; j++)
                                {
                                    expectedDataChecksum = (short)((expectedDataChecksum + contents[i + gap2Offset+10 + j]) % 0xFFFF);
                                }
                                short actualDataChecksum = (short)(contents[i + gap2Offset + 10 + 128 + 1] << 8);
                                actualDataChecksum += contents[i + gap2Offset + 10 + 128];
                                if (ignoreCheckSumErrors || expectedDataChecksum == actualDataChecksum)
                                {
                                    //checksum matches
                                    //sector found
                                    //add to list
                                    var sector = new Sector
                                    {
                                        trackNo = trackNo,
                                        sectorNo = sectorNo,
                                        startIndex = i,
                                        dataStartIndex = i + gap2Offset + 10,
                                        dataLength = 128,
                                        length = 128 + gap2Offset + 9 + 2,
                                        data = new byte[128] 
                                    };
                                    Array.Copy(contents, sector.dataStartIndex, sector.data, 0, sector.dataLength);
                                    sectors.Add(sector);

                                    i += sector.length;
                                }
                                else
                                {
                                    Console.WriteLine($"Data Checksum fail. Track:{trackNo} Sector:{sectorNo} Expected Checksum: {expectedDataChecksum:X4}, Actual Checksum: {actualDataChecksum:X4}");
                                }
                            }
                            if (gap2Found) break;
                        }
                        if (!gap2Found)
                        {
                            Console.WriteLine($"GAP2 fail. Track:{trackNo} Sector:{sectorNo}");

                        }
                    } else
                    {
                        Console.WriteLine($"IDAM Checksum fail. Track:{trackNo} Sector:{sectorNo} Checksum:{checksum} Calculated:{calculatedChecksum}");
                    }
                    
                }
            }
            return sectors;
        }

        byte[] IMemoryAccessor.GetContents(int startAddress, int length)
        {
            throw new NotImplementedException();
        }

        void IMemoryAccessor.SetContents(int startAddress, byte[] contents, int startIndex, int? length)
        {
            throw new NotImplementedException();
        }
    }
}
