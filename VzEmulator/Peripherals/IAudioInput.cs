namespace VzEmulator.Peripherals
{
    public interface IAudioInput
    {
        byte HandleMemoryRead(ushort address);
    }
}