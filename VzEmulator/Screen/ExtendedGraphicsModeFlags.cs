using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulator.Screen
{
    [Flags]
    public enum ExtendedGraphicsModeFlags : byte
    {
        None = 0,
        BankSwitch1 = 1,
        BankSwitch2 = 2,
        GM0 = 4,
        GM1 = 8,
        GM2 = 16,
        SG6 = 32,
        CSS_BackColour = 64,
        G_A_graphics = 128
    }
    //Extended graphics
    //bits 0-5 set from extendedGraphicsLatch.value
    //bits 0,1 are bank switching. Handled in VideoMemory.cs rather than here
    //bit 2 (4)  => 6847 GM0
    //bit 3 (8)  => 6847 GM1 (Held high in unmodified vz. For compatiblity with unmodified, this could be defaulted high) 
    //bit 4 (16) => 6847 GM2
    //bit 5 (32)=> /INT/EXT (0 for Internal chargen/Semigraphics 4, 1 for External chargen/Semigraphics 6)
    //bit 6,7 set from original output latch values for graphics mode & backcolor
    //
    // GM0,1,2 Mode
    //0,0,0 CG1 - 64x64 4 Colour. 3 scan lines per pixel
    //0,0,1 RG1 - 128x64 2 Colour
    //0,1,0 CG2 - 128x64 4 Colour
    //0,1,1 RG2 - 128x96 2 Colour. 2 scan lines per pixel
    //1,0,0 CG3 - 128x96 4 Colour
    //1,0,1 RG3 - 128x192 2 Colour. 1 scan line per pixe
    //1,1,0 CG6 - 128x192 4 Colour
    //1,1,1 RG6 - 256x192 2 Colour
}
