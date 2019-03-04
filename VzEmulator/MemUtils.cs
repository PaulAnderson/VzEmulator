using Konamiman.Z80dotNet;
using System;

namespace VzEmulate2
{
    internal class MemUtils  
    {
        public IMemory Memory;

        public MemUtils(IMemory memory)
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
    }
}
