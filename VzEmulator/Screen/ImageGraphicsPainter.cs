using System;
using System.Collections.Generic;
using System.Drawing;
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

            var bitmap = new Bitmap(256, 192);

            var gr = Graphics.FromImage(bitmap);

            if (ExtendedGraphicsEnabled)
            {
                if ((ExtendedGraphicsMode & ScreenConstants.ExtendedGraphicsModeFlags.GM0) > 0)
                {
                    GraphicsModeRenderer.Render(gr,ExtendedGraphicsMode);
                }
                else
                {
                    TextModeRenderer.Render(gr,ExtendedGraphicsMode);
                }
            }
            else
            {
                TextModeRenderer.Render(gr, ExtendedGraphicsMode);
            }

            return bitmap;
        }
    }
}
