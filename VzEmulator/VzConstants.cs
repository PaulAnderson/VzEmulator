using System;
using System.Drawing;

namespace VzEmulator
{
    internal class VzConstants
    {
        internal const ushort UserExecWordPtr = 0x788E;
        internal const ushort StartBasicProgramPtr = 0x78A4;
        internal const ushort EndBasicProgramPtr = 0x78F9;
        internal const ushort DimVariablesPtr = 0x78FB;
        internal const ushort EndOfBasicStackPtr = 0x78A0;
        internal const ushort InterruptHookPtr = 0x78FD;//E,F
        internal const ushort StartOfInputBufferPtr = 0x79E8;
        internal const ushort CopyOfOutputLatchPtr = 0x783B;
        internal const ushort CursorPositionPtr = 0x78A6;
        internal const ushort OutputDevicePtr = 0x789C;

        internal const ushort OutputLatchAndKbStart = 0x6800;
        internal const ushort OutputLatchAndKbEnd= 0x6FFF;

        internal const ushort VideoRamStart = 0x7000;
        internal const ushort VideoRamEnd = 0x77ff;
        internal const ushort VideoRamSize = 0x7FF;

        internal const ushort TopOfRom = 0x5FFF;


        internal const byte FileTypeBasic = 0xF0;
        internal const byte FileTypeMC = 0xF1;

        internal const byte LightColorCharOffset = 0x40;
        internal const byte ExtendedGraphicsLatchPortStart = 32;
        internal const byte ExtendedGraphicsLatchPortEnd = 47;
        internal const byte ExtendedGraphicsLatchDefault = 0x08; //6847.GM1 for 128x64 Color Graphics


        internal class Colour
        {
            public static Color VZ_BLACK        => Color.FromArgb(0,0,0);
            public static Color VZ_GREEN        => Color.FromArgb(0,255,0);
            public static Color VZ_YELLOW       => Color.FromArgb(255,255,0);
            public static Color VZ_BLUE         => Color.FromArgb(0,0,255);
            public static Color VZ_RED          => Color.FromArgb(255,0,0);
            public static Color VZ_BUFF         => Color.FromArgb(224,224,144);
            public static Color VZ_CYAN         => Color.FromArgb(0,192,160);
            public static Color VZ_MAGENTA      => Color.FromArgb(255,0,255);
            public static Color VZ_ORANGE       => Color.FromArgb(240,112,0);
            public static Color VZ_DK_GREEN     => Color.FromArgb(0,64,0);
            public static Color VZ_BR_GREEN     => Color.FromArgb(0,224,24);
            public static Color VZ_DK_ORANGE    => Color.FromArgb(64,16,0);
            public static Color VZ_BR_ORANGE    => Color.FromArgb(255,196,24);
        }

        [Flags]
        internal enum OutputLatchBits : byte
        {
            GraphicsMode=0x08,
            BackgroundColour=0x10,
        }

    }
}
