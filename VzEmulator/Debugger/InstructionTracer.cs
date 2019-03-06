using Konamiman.Z80dotNet;
using System;


namespace VzEmulator.Debug
{
    internal class InstructionTracer
    {
        IZ80Processor _z80;

        public InstructionTracer(IZ80Processor z80)
        {
            _z80 = z80;
        }
        public void TraceNextInstruction()
        {
            var instruction = GetNextInstruction(_z80);
            var flags = GetFlags(_z80);
            WriteTrace(_z80, instruction, flags);
        }

        private object GetNextInstruction(IZ80Processor z80)
        {
            var instructionLength = GetNextInstructionLength(z80);
            return GetNextInstruction(z80, instructionLength);
        }

        private int GetNextInstructionLength(IZ80Processor z80)
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

        private object GetNextInstruction(IZ80Processor z80, int instructionLength)
        {
            var instruction = "";
            for (int i = 0; i < instructionLength; i++)
            {
                instruction += string.Format("{0:X2}", z80.Memory[z80.Registers.PC + i]);
            }
            instruction = instruction.PadRight(6, ' ');
            return instruction;
        }

        private string GetFlags(IZ80Processor z80)
        {
            var flags = Convert.ToString(z80.Registers.F, 2).PadLeft(8, '0');
            flags = flags.Substring(0, 2) + "X" + flags.Substring(3, 1) + "X" + flags.Substring(5, 3);
            return flags;
        }

        private void WriteTrace(IZ80Processor z80, object instruction, string flags)
        {
            Console.WriteLine("{0:X4} : {1} FLG: {2} A:{3:X2} BC:{4:X4} DE:{5:X4} HL:{6:X4} IX:{7:X4} IY:{8:X4}",
            z80.Registers.PC,
            instruction,
            flags,
            z80.Registers.A, z80.Registers.BC,
            z80.Registers.DE, z80.Registers.HL,
            z80.Registers.IX, z80.Registers.IY);
        }
    }
}
