using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VzEmulator.Peripherals
{
    public class Disk
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
        public int Size = 99200;
        public int TrackLength = 2480;   // 99200/40
        public int Tracks = 40;
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
        public int Length { get
            {
                return contents.Length;
            }
        }
        
        public Disk()
        {
            contents = new Byte[Size];
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
        public void SaveDiskImage(string fileName)
        {
            File.WriteAllBytes(fileName, contents);
            this.fileName = fileName;
        }

        public struct Sector
        {
            public int trackNo;
            public int sectorNo;
            public int startIndex;
            public int dataStartIndex;
            public int dataLength;
            public int length;
        }
        public IEnumerable<Sector> FindSoftSectors()
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
                                if (expectedDataChecksum == actualDataChecksum)
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
                                        length = 128 + gap2Offset + 9 + 2
                                    };
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

    }
}
