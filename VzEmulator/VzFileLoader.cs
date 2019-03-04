using System.IO;

namespace VzEmulate2
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
