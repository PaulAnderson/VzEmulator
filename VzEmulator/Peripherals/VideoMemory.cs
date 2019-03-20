using System;

namespace VzEmulator.Peripherals
{
    class VideoMemory : IPeripheral

    {
        public Tuple<ushort, ushort> PortRange => null;

        public Tuple<ushort, ushort> MemoryRange => new Tuple<ushort, ushort>(VzConstants.VideoRamStart, VzConstants.VideoRamEnd);
        
        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public byte[] Content;

        private IMemoryAccessor Memory;
        public VideoMemory(IMemoryAccessor memory)
        {
            Content = new byte[VzConstants.VideoRamSize + 1];
            this.Memory = memory;
        }

        public void UpdateVideoMemoryFromMainMemory()
        {
            for (int i = VzConstants.VideoRamStart; i <= VzConstants.VideoRamEnd; i++)
            {
                Content[i - VzConstants.VideoRamStart] = Memory[i];
            }
        }


        public byte? HandleMemoryRead(ushort address)
        {
            return null;
        }

        public bool HandleMemoryWrite(ushort address, byte value)
        {
            if (MemoryRange.Item1 <= address & address <= MemoryRange.Item2)
            {
                Content[address - VzConstants.VideoRamStart] = value;
            }
            return false;
        }

        public byte? HandlePortRead(ushort address)
        {
            throw new NotImplementedException();
        }

        public void HandlePortWrite(ushort address, byte value)
        {
            throw new NotImplementedException();
        }
    }
}
