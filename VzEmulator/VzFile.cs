using System;

namespace VzEmulator
{
    class VzFile
    {
        public const int FileNameLength = 17;
        public const int ProgramContentStart = 24;

        public VzFile ()
        {
        }
        public VzFile(int ContentLength)
        {
            content = new byte[ContentLength + ProgramContentStart  ];
        }


        public int header //4 bytes 0x2020  20 20 00 00
        {
            get { return content[0] | (content[1] << 8) | (content[2] << 16) | (content[3] << 24); }
            set { content[0] = (byte)(value & 0xFF);
                content[1] = (byte)((value & 0xFF00) >> 8);
                content[2] = (byte)((value & 0xFF0000) >> 16);
                content[3] = (byte)((value & 0xFF000000) >> 24);
            }
        }
        public byte[] Filename {
            get {
                byte[] subArray = new byte[FileNameLength];
                Array.Copy(content, ProgramContentStart, subArray, 0, FileNameLength);
                return subArray;
            }
            set {
                var length = FileNameLength;
                if (value.Length < length) length = value.Length;
                Array.Copy(value, 0, content, ProgramContentStart, length);
            }
        }
        public byte fileType
        {
            get { return content[21]; }
            set { content[21] = value; }
        }
        public byte startaddr_l
        {
            get { return content[22]; }
            set { content[22] = value; }
        }
        public byte startaddr_h
        {
            get { return content[23]; }
            set { content[23] = value; }
        }
        public byte[] content;

    };
}
