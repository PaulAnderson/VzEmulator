using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator
{
    public class Z80dotNetMemoryAccessor : IMemoryAccessor
    {
        IMemory indexedObject;
        public Z80dotNetMemoryAccessor(IMemory indexedObject) {
            this.indexedObject = indexedObject;
        }

        public byte this[int address]
        {
            get => indexedObject[address];
            set => indexedObject[address] = value;
        }

        public int Size { get => indexedObject.Size; }

        public byte[] GetContents(int startAddress, int length) => indexedObject.GetContents(startAddress, length);
        public void SetContents(int startAddress, byte[] contents, int startIndex = 0, int? length = null)
            => indexedObject.SetContents(startAddress, contents, startIndex, length);
    }
}
