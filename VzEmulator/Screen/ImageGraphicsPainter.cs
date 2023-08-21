using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VzEmulator.Peripherals;

namespace VzEmulator.Screen
{
    internal class ImageGraphicsPainter : GraphicsPainter
    {
        public ImageGraphicsPainter(byte[] VideoMemory, ILatchValue outputLatch, int RenderStartAddress, ILatchValue extendedGraphicsLatch = null) 
            : base(VideoMemory, outputLatch, RenderStartAddress, extendedGraphicsLatch)
        {

        }
        public Bitmap GetImage()
        {
            SetModeFromOutputLatch(OutputLatch?.Value ?? 0);
            bool isGraphicsMode = ExtendedGraphicsMode.HasFlag(ScreenConstants.ExtendedGraphicsModeFlags.G_A_graphics);

            if (isGraphicsMode)
            {
                //todo scale pixels
                var bitmap = GraphicsModeRenderer.Render(null, ExtendedGraphicsMode);

                return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
            else
            {
                return TextModeRenderer.Render(null, ExtendedGraphicsMode);
            }
        }
    }
}
