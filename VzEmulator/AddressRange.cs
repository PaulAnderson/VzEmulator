namespace VzEmulator
{
    public class AddressRange
    {
        public ushort Start { get; set; }
        public ushort End { get; set; }

        public AddressRange(ushort start, ushort end)
        {
            Start = start;
            End = end;
        }
    }
}
