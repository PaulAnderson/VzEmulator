﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VzEmulator.Peripherals
{
    public class PortLatch : IPeripheral , ILatchValue
    {
        public ILatchValue LinkedLatch;
        public byte LinkedLatchCopyMask;

        public Tuple<ushort, ushort> PortRange => _portRange;

        public Tuple<ushort, ushort> MemoryRange => null;

        public bool DebugEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private Tuple<ushort, ushort> _portRange;
        
        public byte Value { get; set; }

        public byte? DefaultValue { get; set; }

        public PortLatch(byte PortRangeStart, byte PortRangeEnd, byte defaultValue)
        {
            _portRange = new Tuple<ushort, ushort>(PortRangeStart, PortRangeEnd);
            Value = defaultValue;
            this.DefaultValue = defaultValue;

        }
        public PortLatch(byte PortRangeStart, byte PortRangeEnd)
        {
            _portRange = new Tuple<ushort, ushort>(PortRangeStart, PortRangeEnd);
        }

        public PortLatch(byte portAddress)
        {
            _portRange = new Tuple<ushort, ushort>(portAddress, portAddress);
        }

        public byte? HandleMemoryRead(ushort address)
        {
            return null;
        }

        public bool HandleMemoryWrite(ushort address, byte value)
        {
            return false;
        }

        public byte? HandlePortRead(ushort address)
        {
            return null;
        }

        public void HandlePortWrite(ushort address, byte value)
        {
            if (_portRange.Item1 <= address & address <= _portRange.Item2)
            {
                SetValue(value);

            }
        }

        private void SetValue(byte value)
        {
            this.Value = value;

            if (LinkedLatch != null)
            {
                LinkedLatch.Value &= (byte)(LinkedLatchCopyMask ^ 0xFF); //Clear bits to copy
                LinkedLatch.Value |= (byte)(value & LinkedLatchCopyMask); //Copy bits
            }
        }

        public void Reset()
        {
            SetValue(this.DefaultValue.GetValueOrDefault());
        }
    }
}
