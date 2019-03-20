using System.IO;

namespace VzEmulator
{
    class VzFileLoader
    {
        public VzFile LoadFile(string FileName)
        {
            return LoadFile(File.ReadAllBytes(FileName));
        }
        public VzFile LoadFile(byte[] content)
        {
            return new VzFile()
            {
                content = content
            };

        }
    }
}
