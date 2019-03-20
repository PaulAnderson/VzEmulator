namespace VzEmulator
{
    public interface IIndexer
    {
        byte this[int address] { get; set; }
    }
    public interface IMemoryAccessor : IIndexer
    {
        int Size { get; }
        byte[] GetContents(int startAddress, int length);
        void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null);
    }
}