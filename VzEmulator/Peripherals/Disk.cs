using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public const int Size = 99200;
        public const int TrackLength = 2480;   // 99200/40
        public const int Tracks = 40;
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
        }
        public void SaveDiskImage(string fileName)
        {
            File.WriteAllBytes(fileName, contents);
            this.fileName = fileName;
        }
    }
}
