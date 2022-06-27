using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using VzEmulator.Peripherals;

namespace VzEmulator.Screen
{
    public enum GraphicsMode
    {
        Text = 0,
        Graphics = 1
    }
    public enum BackgroundColour
    {
        Green = 0,
        Orange = 1
    }
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

        public GraphicsMode GraphicsMode { get; set; }
        public BackgroundColour BackgroundColour { get; set; }

         
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
            //bitmap display, graphics and text modes
            paintControl?.Invalidate();
            paintControl?.Update();

            fps++;

        }

        private void SetModeFromOutputLatch(byte OutputLatchValue)
        {
            GraphicsMode = (OutputLatchValue & (byte)VzConstants.OutputLatchBits.GraphicsMode) > 0 ? GraphicsMode.Graphics : GraphicsMode.Text;
            BackgroundColour = (OutputLatchValue & (byte)VzConstants.OutputLatchBits.BackgroundColour) > 0 ? BackgroundColour.Orange : BackgroundColour.Green;
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

            bool isGraphicsMode = GraphicsMode == GraphicsMode.Graphics;

            if (isGraphicsMode)
                    GraphicsModeRenderer?.Render(gr, BackgroundColour == BackgroundColour.Orange);
            else
               TextModeRenderer?.Render(gr, BackgroundColour == BackgroundColour.Orange);
        }
    }
}
