using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VzEmulator.Screen
{
    class GraphicsModeRenderer : Renderer
    {

        //Defaults for Mode 0 (CG2 Mode)
        protected override int Width => 256;  // (/2 for CG2)
        protected override int Height => 192; // (/3 for CG2)
        private byte PixelsPerByte = 4;
        private byte WidthInBytes = 32;
        private byte PixelSizeX = 2;
        private byte PixelSizeY = 3;

        private new const float AspectRatio = (float)1;

        private int CSSColourValue;
        static Color[] colours => new Color[]
        {
            VzConstants.Colour.VZ_BR_GREEN,
            VzConstants.Colour.VZ_YELLOW,
            VzConstants.Colour.VZ_BLUE,
            VzConstants.Colour.VZ_RED,
            VzConstants.Colour.VZ_BUFF,
            VzConstants.Colour.VZ_CYAN,
            VzConstants.Colour.VZ_MAGENTA,
            VzConstants.Colour.VZ_ORANGE,
        };


        public GraphicsModeRenderer(byte[] Memory, ushort VideoMemoryStartAddress)
            : base(Memory, VideoMemoryStartAddress)
        {
            _GraphicsBitmap = new DirectBitmap(Width, Height);
        }

        public override void Render(Graphics gr, ExtendedGraphicsModeFlags ModeFlags)
        {
            gr.CompositingMode = CompositingMode.SourceCopy;
            gr.CompositingQuality  = CompositingQuality.HighSpeed;
            gr.InterpolationMode = InterpolationMode.NearestNeighbor;

            CSSColourValue  = ModeFlags.HasFlag(ExtendedGraphicsModeFlags.CSS_BackColour) ? 4 : 0;

            RenderGraphicsMode(ModeFlags);
            CopyGraphicsBitmap(gr, _GraphicsBitmap, AspectRatio);

        }

        private void RenderGraphicsMode(ExtendedGraphicsModeFlags ModeFlags)
        {

            //todo change resolution based on mode select
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < WidthInBytes; x++)
                {
                    var pos = (ushort)(y * WidthInBytes + x);
                    byte current = _Memory[_VideoMemoryStartAddress + pos];

                    var mask = 0b00000011;
                    var StartGroupx = x << 2;

                    for (int i=0; i< PixelsPerByte; i++)
                    {
                        var pixelValue = (current & mask) >> (i * 2);
                        var pixelColor = GetPixelColor(pixelValue);
                        _GraphicsBitmap.SetPixel(StartGroupx+3-i, y, pixelColor);
                        mask <<= 2;
                    }

                }
            }
        }

        private Color GetPixelColor(int colourNumber)
        {
            return colours[colourNumber + CSSColourValue];
        }
    }
}
