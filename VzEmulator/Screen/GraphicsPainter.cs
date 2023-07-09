using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using VzEmulator.Peripherals;

namespace VzEmulator.Screen
{

    public class GraphicsPainter
    {
        public event EventHandler RefreshedEvent;
        protected virtual void RaiseRefreshedEvent()
        {
            RefreshedEvent?.Invoke(this, EventArgs.Empty);
        }

        Control paintControl;

        public Renderer TextModeRenderer { get; private set; }
        public Renderer GraphicsModeRenderer { get; private set; }

        Timer screenRefreshTimer;
        Timer fpsCalculateTimer;

        readonly ILatchValue outputLatch;

        readonly ILatchValue extendedGraphicsLatch;
        public bool ExtendedGraphicsEnabled { get; set; } = true;

        int fps;
        public int CurrentFps;

        public bool UseFixedScale {
            set {
                TextModeRenderer.UseFixedScale = value;
                GraphicsModeRenderer.UseFixedScale = value;
            }
            get
            {
                return GraphicsModeRenderer.UseFixedScale;
            }
        }
        public int FixedScale {
            set {
                TextModeRenderer.FixedScale = value;
                GraphicsModeRenderer.FixedScale = value;
            }
            get
            {
                return GraphicsModeRenderer.FixedScale;
            }
        }
        public bool UseSmoothing {
            set {
                TextModeRenderer.UseSmoothing = value;
                GraphicsModeRenderer.UseSmoothing = value;
            }
            get
            {
                return GraphicsModeRenderer.UseSmoothing;
            }
        }

        public ExtendedGraphicsModeFlags ExtendedGraphicsMode { get; set; }
         
        private bool _grayScale;
        public bool GrayScale
        {
            get => _grayScale;
            set
            {
                _grayScale = value;
                if (value)
                {
                    TextModeRenderer.SetGrayScaleImageAttributes();
                    GraphicsModeRenderer.SetGrayScaleImageAttributes();
                }
                else
                {
                    TextModeRenderer.SetDefaultImageAttributes();
                    GraphicsModeRenderer.SetDefaultImageAttributes();
                }
            }
        }

        internal GraphicsPainter(Control ctrl, Byte[] VideoMemory, ILatchValue outputLatch, int RenderStartAddress, int RefreshIntervalMS, ILatchValue  extendedGraphicsLatch = null )
        {
            this.outputLatch = outputLatch;
            this.extendedGraphicsLatch = extendedGraphicsLatch;

            TextModeRenderer = new TextModeRenderer(VideoMemory, 0);
            GraphicsModeRenderer = new GraphicsModeRenderer(VideoMemory, 0);

            if (ctrl != null)
            {
                paintControl = ctrl;
                paintControl.Paint += new System.Windows.Forms.PaintEventHandler(Control_Paint);

                SetupRefreshTimer(RefreshIntervalMS: RefreshIntervalMS);
                SetupFpsCalculator(RefreshIntervalMS: 1000);
            }
        }

        private void SetupRefreshTimer(int RefreshIntervalMS)
        {
            screenRefreshTimer = new Timer {Interval = RefreshIntervalMS};
            screenRefreshTimer.Tick += (v, a) => RenderScreen();
            screenRefreshTimer.Start();
        }
        private void SetupFpsCalculator(int RefreshIntervalMS)
        {
            fpsCalculateTimer = new Timer {Interval = RefreshIntervalMS};
            //ms
            fpsCalculateTimer.Tick += (v, a) =>
            {
                CurrentFps = (int)(fps / (fpsCalculateTimer.Interval / (decimal)RefreshIntervalMS));
                fps = 0;
            };
            fpsCalculateTimer.Start();
        }

        private void RenderScreen()
        {
            var s = Stopwatch.StartNew();
            //bitmap display, graphics and text modes
            paintControl?.Invalidate();
            paintControl?.Update();

            fps++;
            s.Stop();

            //Console.WriteLine($"Screen render took {s.ElapsedMilliseconds} ms ({s.ElapsedTicks} ticks)");
        }

        /// <summary>
        /// Set Flags in ExtendedGraphicsMode based on the contents of the output latch and the extended graphics latch
        /// The intention is for ExtendedGraphicsMode to represent the control inputs to the virtual MC6847
        /// </summary>
        /// <param name="OutputLatchValue"></param>
        private void SetModeFromOutputLatch(byte OutputLatchValue)
        {
            //todo replace with settings
            const bool AuExtendedGraphicsEnabled = true;
            const bool DeExtendedGraphicsEnabled = true;

            if (AuExtendedGraphicsEnabled)
            {
                ExtendedGraphicsMode = (ExtendedGraphicsModeFlags)(extendedGraphicsLatch?.Value ?? (int)ExtendedGraphicsModeFlags.GM1);
            } else
            {
                //if extended graphics mode not enabled, just set GM1 as it was hardwired TRUE In the OG VZ
                ExtendedGraphicsMode = ExtendedGraphicsModeFlags.GM1;
            }

            if (DeExtendedGraphicsEnabled) {
                if ((OutputLatchValue & (byte)VzConstants.OutputLatchBits.Spare_DeModGraphicsMode) > 0)
                {
                    ExtendedGraphicsMode |= ExtendedGraphicsModeFlags.GM0;
                    ExtendedGraphicsMode |= ExtendedGraphicsModeFlags.GM1;
                    ExtendedGraphicsMode |= ExtendedGraphicsModeFlags.GM2;
                }
            }

            if ((OutputLatchValue & (byte)VzConstants.OutputLatchBits.GraphicsMode) > 0)
                ExtendedGraphicsMode |= ExtendedGraphicsModeFlags.G_A_graphics;
            if ((OutputLatchValue & (byte)VzConstants.OutputLatchBits.BackgroundColour) > 0)
                ExtendedGraphicsMode |= ExtendedGraphicsModeFlags.CSS_BackColour;
            
        }

        private void Control_Paint(object sender, PaintEventArgs e)
        {
            var gr = e.Graphics;
            PaintGraphics(gr);
            RaiseRefreshedEvent();
        }

        internal void PaintGraphics(System.Drawing.Graphics gr)
        {
            SetModeFromOutputLatch(outputLatch?.Value ?? 0);

            gr.InterpolationMode = InterpolationMode.NearestNeighbor;

            bool isGraphicsMode = ExtendedGraphicsMode.HasFlag(ExtendedGraphicsModeFlags.G_A_graphics);

            if (isGraphicsMode)
                GraphicsModeRenderer?.Render(gr, ExtendedGraphicsMode);
            else
               TextModeRenderer?.Render(gr, ExtendedGraphicsMode);
        }
    }
}
