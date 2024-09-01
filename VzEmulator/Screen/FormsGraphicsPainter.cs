using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using VzEmulator.Peripherals;

namespace VzEmulator.Screen
{

    internal class FormsGraphicsPainter : GraphicsPainter
    {
        public event EventHandler RefreshedEvent;
        protected virtual void RaiseRefreshedEvent()
        {
            RefreshedEvent?.Invoke(this, EventArgs.Empty);
        }

        Control paintControl;

        Timer screenRefreshTimer;
        Timer fpsCalculateTimer;

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

        public IAnnotator Annotator
        {
            set {
                TextModeRenderer.Annotator = value;
            }
        }

         
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

        internal FormsGraphicsPainter(Control ctrl, Byte[] VideoMemory, ILatchValue outputLatch, int RenderStartAddress, int RefreshIntervalMS, ILatchValue  extendedGraphicsLatch = null )
        : base(VideoMemory,outputLatch, RenderStartAddress, extendedGraphicsLatch)
        {
            if (ctrl != null)
            {
                paintControl = ctrl;
                paintControl.Paint += new System.Windows.Forms.PaintEventHandler(Control_Paint);

                SetupRefreshTimer(RefreshIntervalMS: RefreshIntervalMS);
                SetupFpsCalculator(RefreshIntervalMS: 1000);
            }
        }

        internal FormsGraphicsPainter(Control ctrl, Byte[] VideoMemory,int RefreshIntervalMS, int screenSizeX = 32, int screenSizeY = 16 )
       : base(VideoMemory, null, 0, null)
        {
            ((TextModeRenderer)TextModeRenderer).ScreenSizeX= screenSizeX;
            ((TextModeRenderer)TextModeRenderer).ScreenSizeY = screenSizeY;

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



        private void Control_Paint(object sender, PaintEventArgs e)
        {
            var gr = e.Graphics;
            PaintGraphics(gr);
            RaiseRefreshedEvent();
        }

        internal void PaintGraphics(System.Drawing.Graphics gr)
        {
            SetModeFromOutputLatch(OutputLatch?.Value ?? 0);

            gr.InterpolationMode = InterpolationMode.NearestNeighbor;

            bool isGraphicsMode = ExtendedGraphicsMode.HasFlag(ScreenConstants.ExtendedGraphicsModeFlags.G_A_graphics);

            if (isGraphicsMode)
                GraphicsModeRenderer?.Render(gr, ExtendedGraphicsMode);
            else
               TextModeRenderer?.Render(gr, ExtendedGraphicsMode);
        }
    }
}
