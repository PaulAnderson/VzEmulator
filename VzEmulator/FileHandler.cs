using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VzEmulator.Peripherals;

namespace VzEmulator
{
    internal class FileHandler
    {
        private FileIO reader { get; set; }
        private IMemoryAccessor memory { get; set; }
        private VideoMemory videoMemory { get; set; }

        public FileHandler(FileIO fileReader, IMemoryAccessor memory, VideoMemory videoMemory)
        {
            this.reader = fileReader;
            this.memory = memory;
            this.videoMemory = videoMemory;
        }

        public void LoadFile(string Filename)
        {
            var file = reader.LoadFile(Filename);
            if (file.header == 0x2020 | file.header == 0x30465a56)
            {
                //valid vz file
                 //get start address
                ushort addr = (ushort)file.startaddr_h;
                addr <<= 8;
                addr |= file.startaddr_l;
                ushort addrEnd;
                unchecked
                {
                    addrEnd = (ushort)(addr + file.content.Length - VzFile.ProgramContentStart);
                }

                //load content
                for (int i = 0; i < file.content.Length - VzFile.ProgramContentStart; i++)
                {
                    memory[addr + i] = file.content[i + VzFile.ProgramContentStart];
                }

                //Save start address pointer
                if (file.fileType == VzConstants.FileTypeBasic)
                {
                    //basic file
                    memory[VzConstants.StartBasicProgramPtr] = file.startaddr_l;
                    memory[VzConstants.StartBasicProgramPtr + 1] = file.startaddr_h;

                }
                else if (file.fileType == VzConstants.FileTypeMC)
                {
                    //Machinecode file
                    memory[VzConstants.UserExecWordPtr] = file.startaddr_l;
                    memory[VzConstants.UserExecWordPtr + 1] = file.startaddr_h;

                    // cpu.ExecuteCall(addr);
                }
                else
                {
                    //unknown file type
                }
                //save end address pointer
                if (file.fileType == 0xF0)
                {
                    //basic file
                    memory[0x78F9] = (byte)(addrEnd & 0x00FF);
                    memory[0x78FA] = (byte)((addrEnd & 0xFF00) >> 8);
                    memory[0x78FB] = (byte)(addrEnd & 0x00FF);
                    memory[0x78FC] = (byte)((addrEnd & 0xFF00) >> 8);
                    memory[0x78FD] = (byte)(addrEnd & 0x00FF);
                    memory[0x78FE] = (byte)((addrEnd & 0xFF00) >> 8);
                }
            }
            else
            {
                //invalid file
            }

            //Refresh video memory in case the loaded file overlapped with video memory.
            videoMemory.UpdateVideoMemoryFromMainMemory();

        }

        public void SaveFile(byte FileType, string Filename, ushort StartAddress, ushort EndAddress)
        {
            var length = EndAddress - StartAddress;
            var file = new VzFile(length);
            file.header = 0x2020;
            file.fileType = FileType;
            unchecked
            {
                file.startaddr_h = (byte)((StartAddress & 0xFF00) >> 8);
                file.startaddr_l = (byte)(StartAddress & 0xFF);
            }
            for (int i = 0; i < file.content.Length - VzFile.ProgramContentStart; i++)
            {
                file.content[i + VzFile.ProgramContentStart] = memory[StartAddress + i];
            }

            reader.WriteFile(Filename, file.content);

        }

    }
}
