using Konamiman.Z80dotNet;
using System;


namespace VzEmulator.Debug
{
    internal class InstructionTracer
    {
        ICpu _z80;

        public InstructionTracer(ICpu z80)
        {
            _z80 = z80;
        }
        public void TraceNextInstruction()
        {
            var instruction = GetNextInstruction(_z80);
            var flags = GetFlags(_z80.Registers);
            WriteTrace(_z80.Registers, instruction, flags);
        }

        private object GetNextInstruction(ICpu z80)
        {
            var instructionLength = GetNextInstructionLength(z80);
            return GetNextInstruction(z80, instructionLength);
        }

        private int GetNextInstructionLength(ICpu z80)
        {
            var instructionLength = 1;

            if (z80.Memory[z80.Registers.PC] == 0xCB || z80.Memory[z80.Registers.PC] == 0xED)
            {
                instructionLength = 2;
            }
            if (z80.Memory[z80.Registers.PC] == 0xDD || z80.Memory[z80.Registers.PC] == 0xFD)
            {
                if (z80.Memory[z80.Registers.PC + 1] == 0xED)
                    instructionLength = 3;
                else
                    instructionLength = 2;
            }
            return instructionLength;
        }

        private object GetNextInstruction(ICpu z80, int instructionLength)
        {
            var instruction = "";
            for (int i = 0; i < instructionLength; i++)
            {
                instruction += string.Format("{0:X2}", z80.Memory[z80.Registers.PC + i]);
            }
            instruction = instruction.PadRight(6, ' ');
            return instruction;
        }

        private string GetFlags(IRegisters registers)
        {
            var flags = Convert.ToString(registers.F, 2).PadLeft(8, '0');
            flags = flags.Substring(0, 2) + "X" + flags.Substring(3, 1) + "X" + flags.Substring(5, 3);
            return flags;
        }

        private void WriteTrace(IRegisters registers, object instruction, string flags)
        {
            Console.WriteLine("{0:X4} : {1} FLG: {2} A:{3:X2} BC:{4:X4} DE:{5:X4} HL:{6:X4} IX:{7:X4} IY:{8:X4}",
            registers.PC,
            instruction,
            flags,
            registers.A, registers.BC,
            registers.DE, registers.HL,
            registers.IX, registers.IY);
        }
    }
}
