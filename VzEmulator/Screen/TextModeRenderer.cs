using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using VzEmulator.Screen;

namespace VzEmulator.Screen
{
    class TextModeRenderer : Renderer
    {
        const string FileName = "Fonts/Font.png";
        public DirectBitmap _FontBitmap;

        public int ScreenSizeY { get; set; } = 16;
        public int ScreenSizeX { get; set; } = 32;
        protected int CharSizeY { get; set; } = 12;
        protected int CharSizeX { get; set; } = 8;

        protected override int Width => CharSizeX * ScreenSizeX; //256 for 32 columns
        protected override int Height => CharSizeY * ScreenSizeY; //192 for 16 rows

        public TextModeRenderer(byte[] Memory, ushort VideoMemoryStartAddress)
            : base(Memory, VideoMemoryStartAddress)
        {
            var fontLoader = new PngFontLoader();
            _FontBitmap = fontLoader.LoadFont(FileName);
            _GraphicsBitmap = new DirectBitmap(Width, Height);

        }

        public override Bitmap Render(Graphics gr, ScreenConstants.ExtendedGraphicsModeFlags ModeFlags)
        {
            if (_GraphicsBitmap.Width != Width || _GraphicsBitmap.Height != Height)
            {
                _GraphicsBitmap = new DirectBitmap(Width, Height);
            }
            renderTextMode(ModeFlags.HasFlag(ScreenConstants.ExtendedGraphicsModeFlags.CSS_BackColour), ModeFlags.HasFlag(ScreenConstants.ExtendedGraphicsModeFlags.SG6));
            if (gr != null)
            {
                CopyGraphicsBitmap(gr, _GraphicsBitmap, AspectRatio);
            }
            return _GraphicsBitmap.Bitmap;
        }

        private void renderTextMode(bool background, bool extendedSG6)
        {
            using (var gr = Graphics.FromImage(_GraphicsBitmap.Bitmap))
            {
                int charOffset = 0;
                if (background) charOffset += 96;

                var scale = 1;

                //todo user definable x,y size. Default 32x16
                
                for (int y = 0; y < ScreenSizeY; y++)
                {
                    for (int x = 0; x < ScreenSizeX; x++)
                    {
                        var offset = ScreenSizeX * y + x;
                        var c = _Memory[_VideoMemoryStartAddress + offset];
                        
                        var destx = x * (8 * scale);
                        var desty = y * (12 * scale);

                        var destRect = new Rectangle(destx, desty, 8 * scale, 12 * scale);
                        var destPoints = ToPoints(destRect);

                        if (!extendedSG6)
                        {
                            //Internal chargen & SG4 semigraphics
                            var row = c / 32;
                            var col = c - row * 32;
                            var srcx = col * 8;
                            var srcy = charOffset + row * 12;
                            var srcRect = new Rectangle(srcx, srcy, 8, 12);
                            gr.DrawImage(_FontBitmap.Bitmap, destPoints, srcRect, GraphicsUnit.Pixel, ImageAttributes);

                            if (Annotator != null)
                            {
                                Annotator.PostRender(x, y, offset, scale, c, destRect, gr);
                            }


                        }
                        else
                        {
                            //External chargen & SG6 semigraphics
                            //TODO
                            if ((c & 0x80)==0)
                            {
                                //Render regular chars as normal for now. TODO allow pluggable font for external chargen
                                c ^= 0x40;
                                var row = c / 32;
                                var col = c - row * 32;
                                var srcx = col * 8;
                                var srcy = charOffset + row * 12;
                                var srcRect = new Rectangle(srcx, srcy, 8, 12);
                                gr.DrawImage(_FontBitmap.Bitmap, destPoints, srcRect, GraphicsUnit.Pixel, ImageAttributes);
                            } else
                            {
                                //Semigraphics. 
                                //Todo render semigraphics char and paint to screen
                                var color = c & 0xc0; //most significant 2 bits. 0x80 will always be set because G/A is wired to D7
                                var blocks = c & 0x3f; //least significant 6 bits

                                const int blockwidth = 4; // 8/2
                                const int blockHeight = 4; // 12/3
                                var blockSize = new Size(blockwidth * scale, blockHeight * scale); //Blocks are 2 pixels wide, 3 pixels high (8/2 and 12/3)

                                //block layout
                                //L5 L4
                                //L3 L2
                                //L1 L0
                                Brush bgPen = new SolidBrush(VzConstants.Colour.VZ_BLACK); //todo check
                                Brush cpen0 = background ? new SolidBrush(VzConstants.Colour.VZ_MAGENTA) : new SolidBrush(VzConstants.Colour.VZ_BLUE);  
                                Brush cpen1 = background ? new SolidBrush(VzConstants.Colour.VZ_ORANGE) :new SolidBrush(VzConstants.Colour.VZ_RED); 
                                Brush fgPen = (c & 0x40) == 0 ? cpen1 : cpen0;

                                Point l5 = new Point(destx, desty);
                                Point l4 = new Point(destx + blockwidth * scale, desty);
                                Point l3 = new Point(destx, desty + blockHeight * scale);
                                Point l2 = new Point(destx + blockwidth * scale, desty + blockHeight * scale);
                                Point l1 = new Point(destx, desty + 2 * blockHeight * scale);
                                Point l0 = new Point(destx + blockwidth * scale, desty + 2 * blockHeight * scale);

                                var rect5 = new Rectangle(l5, blockSize);
                                var rect4 = new Rectangle(l4, blockSize);
                                var rect3 = new Rectangle(l3, blockSize);
                                var rect2 = new Rectangle(l2, blockSize);
                                var rect1 = new Rectangle(l1, blockSize);
                                var rect0 = new Rectangle(l0, blockSize);

                                var pen5 = (c & 0x20) == 0 ? bgPen : fgPen;
                                var pen4 = (c & 0x10) == 0 ? bgPen : fgPen;
                                var pen3 = (c & 0x08) == 0 ? bgPen : fgPen;
                                var pen2 = (c & 0x04) == 0 ? bgPen : fgPen;
                                var pen1 = (c & 0x02) == 0 ? bgPen : fgPen;
                                var pen0 = (c & 0x01) == 0 ? bgPen : fgPen;

                                gr.FillRectangle(pen5,rect5);
                                gr.FillRectangle(pen4, rect4);
                                gr.FillRectangle(pen3, rect3);
                                gr.FillRectangle(pen2, rect2);
                                gr.FillRectangle(pen1, rect1);
                                gr.FillRectangle(pen0, rect0);

                            }
                        }
                    }
                }
            }
        }

        public string GetText()
        {

            var sb = new StringBuilder();
            var offset = 0;
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 32; x++)
                {
                    //get value
                    ushort pos;
                    unchecked
                    {
                        pos = (ushort)(offset + y * 32 + x);
                    }
                    var value = _Memory[_VideoMemoryStartAddress + pos];

                    //create string
                    if (value < 0x20) value += 0x40;
                    if (value >= 0x60 && value < 0x80) value -= 0x40;
                    if (value > 32 && value <= 128)
                    {
                        sb.Append((char)value);
                    }
                    else if (value == 32 && _Memory[_VideoMemoryStartAddress + pos] == 32) //inverted space, cursor
                    {
                        sb.Append('#');
                    }
                    {
                        sb.Append(' ');
                    }
                }
                sb.AppendLine("");
            }

            return sb.ToString();
        }   
    }
}
