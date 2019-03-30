using System;

namespace VzEmulator
{
    internal class MemUtils  
    {
        public IIndexer Memory;

        public MemUtils(IIndexer memory)
        {
            Memory = memory;
        }

        public byte GetByteAtAddress(ushort address)
        {
            return Memory[address];
        }
        public void SetByteAtAddress(ushort address,byte value)
        {
            Memory[address]=value;
        }
        public ushort GetWordAtAddress(ushort address)
        {
            var result = (ushort)(Memory[address+1] << 8 | Memory[address]);
            return result;
        }
        public void SetWordAtAddress(ushort address,ushort value)
        {
            Memory[address] = (byte)(value & 0xFF);
            Memory[address+1] = (byte)((value & 0xFF00) >> 8);
        }

        public void SetSWordAtAddress(ushort address, short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Memory[address] = bytes[0];
            Memory[address + 1] = bytes[1];
        }
        public  short GetSWordAtAddress(ushort address)
        {
            byte[] bytes = new byte[2] { Memory[address], Memory[address + 1] };
            return BitConverter.ToInt16(bytes, 0);
        }

        public static ushort StringToByte(String str)
        {
            if (str.ToLower().StartsWith("0x"))
            {
                str = str.Substring(2);
                return ushort.Parse(str, System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                return ushort.Parse(str, System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingWhite);
            }
        }
        public static ushort StringToUShort(String str)
        {
            if (str.ToLower().StartsWith("0x") ) {
                str = str.Substring(2);
                    return ushort.Parse(str, System.Globalization.NumberStyles.HexNumber);
            } else {
                return ushort.Parse(str, System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingWhite);
            }
        }
        public static short StringToShort(String str)
        {
            if (str.ToLower().StartsWith("0x"))
            {
                str = str.Substring(2);
                return short.Parse(str, System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                return short.Parse(str, System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingWhite);
            }
        }

        public ushort SaveRegistersToMemory(IRegisters registers, ushort startAddress)
        {
            ushort address = startAddress;
            SetSWordAtAddress(address, registers.AF);
            address += 2;
            SetSWordAtAddress(address, registers.BC);
            address += 2;
            SetSWordAtAddress(address, registers.DE);
            address += 2;
            SetSWordAtAddress(address, registers.HL);
            address += 2;
            SetSWordAtAddress(address, registers.IX);
            address += 2;
            SetSWordAtAddress(address, registers.IY);
            address += 2;
            SetSWordAtAddress(address, registers.SP);
            address += 2;
            SetWordAtAddress(address, registers.PC);
            address += 2;
            SetSWordAtAddress(address, registers.AltAF);
            address += 2;
            SetSWordAtAddress(address, registers.AltBC);
            address += 2;
            SetSWordAtAddress(address, registers.AltDE);
            address += 2;
            SetSWordAtAddress(address, registers.AltHL);
            return address;
        }
        public ushort LoadRegistersFromMemory(IRegisters registers, ushort startAddress)
        {
            ushort address = startAddress;
            registers.AF = GetSWordAtAddress(address);
            address += 2;
            registers.BC = GetSWordAtAddress(address);
            address += 2;
            registers.DE = GetSWordAtAddress(address);
            address += 2;
            registers.HL = GetSWordAtAddress(address);
            address += 2;
            registers.IX = GetSWordAtAddress(address);
            address += 2;
            registers.IY = GetSWordAtAddress(address);
            address += 2;
            registers.SP = GetSWordAtAddress(address);
            address += 2;
            registers.PC = GetWordAtAddress(address);
            address += 2;
            registers.AltAF = GetSWordAtAddress(address);
            address += 2;
            registers.AltBC = GetSWordAtAddress(address);
            address += 2;
            registers.AltDE = GetSWordAtAddress(address);
            address += 2;
            registers.AltHL = GetSWordAtAddress(address);
            return address;
        }
    }
}
