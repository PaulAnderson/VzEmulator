namespace VzEmulator
{
    public interface IClockSynced
    {
        decimal ClockFrequency { get; set; }
        
        void ProcessClockCycles(int periodLengthInCycles);
    }
}