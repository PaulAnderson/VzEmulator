using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VzEmulator.Screen
{
    class GraphicsModeRenderer : Renderer
    {

        //Defaults for Mode 0 (CG2 Mode)
        private byte PixelsPerByte = 4;
        private byte WidthInBytes = 32;

        private new float AspectRatio = (float)1.5;

        private bool isColourGraphicsMode = true;
        private int CSSColourValue;
       


        public GraphicsModeRenderer(byte[] Memory, ushort VideoMemoryStartAddress)
            : base(Memory, VideoMemoryStartAddress)
        {
            (Width, Height) = (128, 64); 
            SetUpGraphicsBitmap();
        }

        private void SetUpGraphicsBitmap()
        {
            if (_GraphicsBitmap == null || _GraphicsBitmap.Height != Height || _GraphicsBitmap.Width != Width)
            {
                _GraphicsBitmap?.Dispose();
                _GraphicsBitmap = new DirectBitmap(Width, Height);
            }
        }

        public override void Render(Graphics gr, ScreenConstants.ExtendedGraphicsModeFlags ModeFlags)
        {
            gr.CompositingMode = CompositingMode.SourceCopy;
            gr.CompositingQuality = CompositingQuality.HighSpeed;
            gr.InterpolationMode = InterpolationMode.NearestNeighbor;

            SetUpGraphicsMode(ModeFlags);

            SetUpGraphicsBitmap();

            RenderGraphicsMode(ModeFlags);
            CopyGraphicsBitmap(gr, _GraphicsBitmap, AspectRatio);

        }



        private void SetUpGraphicsMode(ScreenConstants.ExtendedGraphicsModeFlags ModeFlags)
        {
            //Colours
            isColourGraphicsMode = !ModeFlags.HasFlag(ScreenConstants.ExtendedGraphicsModeFlags.GM0);
            var cssColourIndexIncrement = isColourGraphicsMode ? 4 : 2;
            CSSColourValue = ModeFlags.HasFlag(ScreenConstants.ExtendedGraphicsModeFlags.CSS_BackColour) ? cssColourIndexIncrement : 0;

            //Resolution/Colour mode based on GM2 (MSB), GM1, GM0 (LSB)
            var ModeNo = (((byte)ModeFlags) & 0b00011100) >> 2;
            switch (ModeNo)
            {
                case 0: //CG1 64x64 4 Colours
                    (Width, Height) = (64, 64);
                    PixelsPerByte = 4;
                    WidthInBytes = 16;
                    AspectRatio = (float).75;
                    break;
                case 
                1: //RG1 128x64 2 Colours
                    (Width, Height) = (128, 64);
                    PixelsPerByte = 8;
                    WidthInBytes = 16;
                    AspectRatio = (float)1.5;
                    break;
                case 2: //CG2 128x64 4 Colours
                    (Width, Height) = (128, 64);
                    PixelsPerByte = 4;
                    WidthInBytes = 32;
                    AspectRatio = (float)1.5;
                    break;
                case 3: //RG2 128x96 2 Colours
                    (Width, Height) = (128, 96);
                    PixelsPerByte = 8;
                    WidthInBytes = 16;
                    AspectRatio = (float)1.5;
                    break;
                case 4: //CG3 128x96 4 Colours
                    (Width, Height) = (128, 96);
                    PixelsPerByte = 4;
                    WidthInBytes = 32;
                    AspectRatio = (float)1.5;
                    break;
                case 5: //RG3 128x192 2 colours
                    (Width, Height) = (128, 192);
                    PixelsPerByte = 8;
                    WidthInBytes = 16;
                    AspectRatio = (float)1.5;
                    break;
                case 6: //CG4 128x192 4 colours
                    (Width, Height) = (128, 192);
                    PixelsPerByte = 4;
                    WidthInBytes = 32;
                    AspectRatio = (float)1.5;
                    break;
                case 7: //RG4 256x192 2 colours
                    (Width, Height) = (256, 192);
                    PixelsPerByte = 8;
                    WidthInBytes = 32;
                    AspectRatio = (float)1;
                    break;
            }
        }

        private void RenderGraphicsMode(ScreenConstants.ExtendedGraphicsModeFlags ModeFlags)
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < WidthInBytes; x++)
                {
                    var mask = 0b00000011;
                    var bitShift = 2;
                    if (PixelsPerByte == 8)
                    {
                        mask = 1;
                        bitShift = 1;
                    }

                    var pos = (ushort)(y * WidthInBytes + x);
                    byte current = _Memory[_VideoMemoryStartAddress + pos];

                    var StartGroupx = x * PixelsPerByte;

                    for (int i = 0; i < PixelsPerByte; i++)
                    {
                        var pixelValue = (current & mask) >> (i * bitShift);
                        var pixelColor = GetPixelColor(pixelValue+CSSColourValue);
                        _GraphicsBitmap.SetPixel(StartGroupx + (PixelsPerByte - 1) - i, y, pixelColor);
                        mask <<= bitShift;
                    }
                }
            }
        }

        private Color GetPixelColor(int colourNumber)
        {
            if (isColourGraphicsMode)
            {
                return ScreenConstants.ColourModecolours[colourNumber];
            }
            else
            {
                return ScreenConstants.ResolutionModecolours[colourNumber];
            }
        }
    }
}
