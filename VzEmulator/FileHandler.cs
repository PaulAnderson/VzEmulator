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

        /// <summary>
        /// Loads a file into memory and sets address pointers
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns>The address range of the </returns>
        public AddressRange LoadFile(string Filename)
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
                }
                else
                {
                    //unknown file type
                }
                //save end address pointer
                if (file.fileType == 0xF0)
                {
                    //basic file
                    memory[VzConstants.EndBasicProgramPtr] = (byte)(addrEnd & 0x00FF);
                    memory[VzConstants.EndBasicProgramPtr + 1] = (byte)((addrEnd & 0xFF00) >> 8);
                    memory[VzConstants.DimVariablesPtr] = (byte)(addrEnd & 0x00FF);
                    memory[VzConstants.DimVariablesPtr + 1] = (byte)((addrEnd & 0xFF00) >> 8);
                    memory[VzConstants.InterruptHookPtr] = (byte)(addrEnd & 0x00FF);
                    memory[VzConstants.InterruptHookPtr + 1] = (byte)((addrEnd & 0xFF00) >> 8);
                }

                //Refresh video memory in case the loaded file overlapped with video memory.
                videoMemory.UpdateVideoMemoryFromMainMemory();
                return new AddressRange(addr, addrEnd);
            }
            else
            {
                //invalid file
                return null;
            }
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
