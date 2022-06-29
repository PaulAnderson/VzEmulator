using System.Drawing;
using System.Drawing.Drawing2D;

namespace VzEmulator.Screen
{
    class GraphicsModeRenderer : Renderer
    {

        protected override int Width => 128;
        protected override int Height => 64;

        private new const float AspectRatio = (float)1.5;

        private const byte PixelsPerByte = 4;
        private const byte WidthInBytes = 32;

        public GraphicsModeRenderer(byte[] Memory, ushort VideoMemoryStartAddress)
            : base(Memory, VideoMemoryStartAddress)
        {
            _GraphicsBitmap = new DirectBitmap(Width, Height);
        }
        public override void Render(Graphics gr, bool background)
        {
            gr.CompositingMode = CompositingMode.SourceCopy;
            gr.CompositingQuality  = CompositingQuality.HighSpeed;
            gr.InterpolationMode = InterpolationMode.NearestNeighbor;

            RenderGraphicsMode();
            CopyGraphicsBitmap(gr, _GraphicsBitmap, AspectRatio);

        }
        private Color GetPixelColor(int value)
        {
            //todo change colour palette based on background colour
            if (value == 0) return VzConstants.Colour.VZ_BR_GREEN;
            else if (value == 1) return VzConstants.Colour.VZ_YELLOW;
            else if (value == 2) return VzConstants.Colour.VZ_BLUE;
            else return VzConstants.Colour.VZ_RED;
        }

        private void RenderGraphicsMode()
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
    }
}
