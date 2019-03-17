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
        public const int Size = 99160;
        public const int TrackLength = 2480;   // 99160/40
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
            contents = File.ReadAllBytes(fileName);
            this.fileName = fileName;
        }
        public void SaveDiskImage(string fileName)
        {
            File.WriteAllBytes(fileName, contents);
            this.fileName = fileName;
        }
    }
}
