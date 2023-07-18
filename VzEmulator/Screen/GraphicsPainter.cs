using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VzEmulator.Peripherals;

namespace VzEmulator.Screen
{

    internal abstract class GraphicsPainter
    {
        public ScreenConstants.ExtendedGraphicsModeFlags ExtendedGraphicsMode { get; set; }

        public bool ExtendedGraphicsEnabled { get; set; } = true;

        public readonly ILatchValue OutputLatch;

        public readonly ILatchValue ExtendedGraphicsLatch;

        public Renderer TextModeRenderer { get; private set; }
        public Renderer GraphicsModeRenderer { get; private set; }

        internal GraphicsPainter(Byte[] VideoMemory, ILatchValue outputLatch, int RenderStartAddress, ILatchValue extendedGraphicsLatch = null)
        {
            OutputLatch = outputLatch;
            ExtendedGraphicsLatch = extendedGraphicsLatch;

            TextModeRenderer = new TextModeRenderer(VideoMemory, 0);
            GraphicsModeRenderer = new GraphicsModeRenderer(VideoMemory, 0);

        }
        /// <summary>
        /// Set Flags in ExtendedGraphicsMode based on the contents of the output latch and the extended graphics latch
        /// The intention is for ExtendedGraphicsMode to represent the control inputs to the virtual MC6847
        /// </summary>
        /// <param name="OutputLatchValue"></param>
        protected void SetModeFromOutputLatch(byte OutputLatchValue)
        {
            //todo replace with settings
            const bool AuExtendedGraphicsEnabled = true;
            const bool DeExtendedGraphicsEnabled = true;

            if (AuExtendedGraphicsEnabled)
            {
                ExtendedGraphicsMode = (ScreenConstants.ExtendedGraphicsModeFlags)(ExtendedGraphicsLatch?.Value ?? (int)ScreenConstants.ExtendedGraphicsModeFlags.GM1);
            }
            else
            {
                //if extended graphics mode not enabled, just set GM1 as it was hardwired TRUE In the OG VZ
                ExtendedGraphicsMode = ScreenConstants.ExtendedGraphicsModeFlags.GM1;
            }

            if (DeExtendedGraphicsEnabled)
            {
                if ((OutputLatchValue & (byte)VzConstants.OutputLatchBits.Spare_DeModGraphicsMode) > 0)
                {
                    ExtendedGraphicsMode |= ScreenConstants.ExtendedGraphicsModeFlags.GM0;
                    ExtendedGraphicsMode |= ScreenConstants.ExtendedGraphicsModeFlags.GM1;
                    ExtendedGraphicsMode |= ScreenConstants.ExtendedGraphicsModeFlags.GM2;
                }
            }

            if ((OutputLatchValue & (byte)VzConstants.OutputLatchBits.GraphicsMode) > 0)
                ExtendedGraphicsMode |= ScreenConstants.ExtendedGraphicsModeFlags.G_A_graphics;
            if ((OutputLatchValue & (byte)VzConstants.OutputLatchBits.BackgroundColour) > 0)
                ExtendedGraphicsMode |= ScreenConstants.ExtendedGraphicsModeFlags.CSS_BackColour;

        }
    }
}
