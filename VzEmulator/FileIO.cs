using System.IO;

namespace VzEmulator
{
    abstract class FileIO
    {
        abstract public VzFile LoadFile(string FileName);
        abstract public VzFile LoadFile(byte[] content);
        abstract public void WriteFile(string fileName, byte[] content);

        public static FileIO GetDefaultImplementation()
        {
            return new FileIOImpl();
        }
    }

    class FileIOImpl : FileIO
    {
        public override VzFile LoadFile(string FileName)
        {
            return LoadFile(File.ReadAllBytes(FileName));
        }
        public override VzFile LoadFile(byte[] content)
        {
            return new VzFile()
            {
                content = content
            };

        }

        public override void WriteFile(string fileName, byte[] content)
        {
            File.WriteAllBytes(fileName, content);
        }
    }
}
