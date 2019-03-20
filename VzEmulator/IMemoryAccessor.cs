namespace VzEmulator
{
    public interface IMemoryAccessor
    {
        byte this[int address] { get; set; }
        int Size { get; }
        byte[] GetContents(int startAddress, int length);
        void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null);
    }
}