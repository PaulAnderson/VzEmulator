using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace VzEmulator.Screen
{
    public abstract class Renderer
    {
        protected Byte[] _Memory;
        protected ushort _VideoMemoryStartAddress;
        protected DirectBitmap _GraphicsBitmap;

        protected virtual int Width { get; set; }
        protected virtual int Height { get; set; }
        protected const float AspectRatio = 1;

        public bool UseFixedScale { get; set; }
        public int FixedScale { get; set; }
        public bool UseSmoothing { get; set; }

        protected ImageAttributes ImageAttributes { get; set; }

        public void SetDefaultImageAttributes()
        {
            ImageAttributes = new ImageAttributes();
        }
        public void SetGrayScaleImageAttributes()
        {

            var matrix = new[]
            {
                new[] { 0.299f, 0.299f, 0.299f, 0, 0 },
                new[] { 0.587f, 0.587f, 0.587f, 0, 0 },
                new[] { 0.114f, 0.114f, 0.114f, 0, 0 },
                new[] { 0f,      0,      0,      1, 0 },
                new[] { 0f,      0,      0,      0, 1 },
            };

            var ia = new ImageAttributes();
            ia.SetColorMatrix(new ColorMatrix(matrix));
            ImageAttributes = ia;
        }
        protected Renderer(byte[] Memory, ushort VideoMemoryStartAddress)
        {
            _Memory = Memory;
            _VideoMemoryStartAddress = VideoMemoryStartAddress;
            if (_Memory.Length - 1 < VideoMemoryStartAddress)
                throw new Exception("VideoMemoryStartAddress outside memory range");

        }

        public abstract Bitmap Render(System.Drawing.Graphics gr, ScreenConstants.ExtendedGraphicsModeFlags ModeFlags);

        protected void CopyGraphicsBitmap(Graphics gr, DirectBitmap graphicsBitmap, float aspectRatio)
        {
            gr.CompositingMode = CompositingMode.SourceCopy;
            if (UseSmoothing)
            {
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBilinear;
            }
            else
            {
                gr.CompositingQuality = CompositingQuality.HighSpeed;
                gr.InterpolationMode = InterpolationMode.NearestNeighbor;
            }

            var margin = 16;
            float scalex;
            float scaley;
            int destx = 0, desty = 0, srcx = 0, srcy = 0;

            var destWidth = gr.ClipBounds.Width - margin * 2;
            var destHeight = gr.ClipBounds.Height - margin * 2;

            scalex = destWidth / Width;
            if (UseFixedScale && scalex > 1) scalex = (int)scalex;
            scaley = scalex * aspectRatio;

            if (Height * scaley > destHeight)
            {
                scaley = destHeight / Height;
                if (UseFixedScale && scaley > 1) scaley = (int)scaley;
                scalex = scaley / aspectRatio;
            }

            if (scalex < 1 || scaley < 1) gr.InterpolationMode = InterpolationMode.HighQualityBilinear; //force smoothing if scaled down

            var srcRect = new Rectangle(srcx, srcy, Width, Height);
            var destRect = new Rectangle(destx + margin, desty + margin, (int)(Width * scalex), (int)(Height * scaley));
            var destPoints = ToPoints(destRect);

            gr.DrawImage(graphicsBitmap.Bitmap, destPoints, srcRect, GraphicsUnit.Pixel, ImageAttributes);

        }

        protected PointF[] ToPoints(Rectangle rectangle)
        {
            var points = new PointF[3]
            {
                new PointF( rectangle.Left,rectangle.Top),
                new PointF( rectangle.Right,rectangle.Top),
                new PointF(rectangle.Left,rectangle.Bottom),
            };
            return points;
        }
    }
}
