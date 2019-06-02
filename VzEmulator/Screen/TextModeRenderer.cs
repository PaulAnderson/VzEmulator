using System.Drawing;
using System.Drawing.Drawing2D;
using VzEmulator.Screen;

namespace VzEmulator.Screen
{
    class TextModeRenderer : Renderer
    {
        const string FileName = "Fonts/Font.png";
        public DirectBitmap _FontBitmap;

        protected override int Width => 256;
        protected override int Height => 192;

        public TextModeRenderer(byte[] Memory, ushort VideoMemoryStartAddress)
            : base(Memory, VideoMemoryStartAddress)
        {
            _GraphicsBitmap = new DirectBitmap(Width, Height);
            var fontLoader = new PngFontLoader();
            _FontBitmap = fontLoader.LoadFont(FileName);
        }

        public override void Render(Graphics gr, bool background)
        {
            renderTextMode(background);
            CopyGraphicsBitmap(gr, _GraphicsBitmap, AspectRatio);
        }

        private void renderTextMode(bool background)
        {
            using (var gr = Graphics.FromImage(_GraphicsBitmap.Bitmap))
            {
                int charOffset = 0;
                if (background) charOffset += 96;

                var scale = 1;

                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        var offset = 32 * y + x;
                        var c = _Memory[_VideoMemoryStartAddress + offset];
                        var row = c / 32;
                        var col = c - row * 32;
                        var srcx = col * 8;
                        var srcy = charOffset + row * 12;
                        var destx = x * (8 * scale);
                        var desty = y * (12 * scale);

                        var srcRect = new Rectangle(srcx, srcy, 8, 12);

                        var destRect = new Rectangle(destx, desty, 8 * scale, 12 * scale);
                        var destPoints = ToPoints(destRect);

                        gr.DrawImage(_FontBitmap.Bitmap, destPoints, srcRect, GraphicsUnit.Pixel, ImageAttributes);
                    }
                }
            }
        }
    }
}
