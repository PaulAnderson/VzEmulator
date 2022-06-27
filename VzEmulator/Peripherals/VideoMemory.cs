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

        private const byte VideoMemoryBankSwitchMask = 3; //1 Less than total number of video banks (power of 2).4 (8KB) to support graphics mod

        public ILatchValue BankSwitchLatch { get; }
        public Byte BankSelect => (byte)(BankSwitchLatch.Value & VideoMemoryBankSwitchMask);
        public bool ExtendedGraphicsEnabled { get; set; } = true;

        public VideoMemory(IMemoryAccessor memory, ILatchValue bankSwitchLatch)
        {
            Content = new byte[(VzConstants.VideoRamSize + 1) * (VideoMemoryBankSwitchMask+1)];
            this.Memory = memory;
            BankSwitchLatch = bankSwitchLatch;
        }

        public void UpdateVideoMemoryFromMainMemory()
        {
            //Todo update this to support bank switch

            for (int i = VzConstants.VideoRamStart; i <= VzConstants.VideoRamEnd; i++)
            {
                Content[i - VzConstants.VideoRamStart] = Memory[i];
            }
        }


        public byte? HandleMemoryRead(ushort address)
        {
            if (MemoryRange.Item1 <= address & address <= MemoryRange.Item2)
            {
                return Content[address - VzConstants.VideoRamStart + CalculateBankOffset()];
            }
            return null;
        }

        public bool HandleMemoryWrite(ushort address, byte value)
        {
                
            if (MemoryRange.Item1 <= address & address <= MemoryRange.Item2)
            {
                Content[address - VzConstants.VideoRamStart + CalculateBankOffset()] = value;
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

        private int CalculateBankOffset()
        {
            int BankOffset = 0;
            if (ExtendedGraphicsEnabled)
            {
                BankOffset = BankSelect * (VzConstants.VideoRamSize + 1);
            }
            return BankOffset;
        }
    }
}
