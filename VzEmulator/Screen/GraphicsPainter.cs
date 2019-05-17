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
        Control paintControl;

        Renderer textModeRenderer;
        Renderer graphicsModeRenderer;

        Timer screenRefreshTimer;
        Timer fpsCalculateTimer;

        OutputLatch outputLatch;

        int fps;
        public String CurrentFps;

        public bool UseFixedScale {
            set {
                textModeRenderer.UseFixedScale = value;
                graphicsModeRenderer.UseFixedScale = value;
            }
            get
            {
                return graphicsModeRenderer.UseFixedScale;
            }
        }
        public int FixedScale {
            set {
                textModeRenderer.FixedScale = value;
                graphicsModeRenderer.FixedScale = value;
            }
            get
            {
                return graphicsModeRenderer.FixedScale;
            }
        }
        public bool UseSmoothing {
            set {
                textModeRenderer.UseSmoothing = value;
                graphicsModeRenderer.UseSmoothing = value;
            }
            get
            {
                return graphicsModeRenderer.UseSmoothing;
            }
        }

        public GraphicsMode GraphicsMode { get; set; }
        public BackgroundColour BackgroundColour { get; set; }

        public GraphicsPainter(Control ctrl, Byte[] VideoMemory, OutputLatch outputLatch, int RenderStartAddress, int RefreshIntervalMS)
        {
            this.outputLatch = outputLatch;
            textModeRenderer = new TextModeRenderer(VideoMemory, 0);
            graphicsModeRenderer = new GraphicsModeRenderer(VideoMemory, 0);

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
            screenRefreshTimer = new Timer();
            screenRefreshTimer.Interval = RefreshIntervalMS;
            screenRefreshTimer.Tick += (v, a) => RenderScreen();
            screenRefreshTimer.Start();
        }
        private void SetupFpsCalculator(int RefreshIntervalMS)
        {
            fpsCalculateTimer = new Timer();
            fpsCalculateTimer.Interval = RefreshIntervalMS; //ms
            fpsCalculateTimer.Tick += (v, a) =>
            {
                CurrentFps = (fps / (fpsCalculateTimer.Interval / (decimal)RefreshIntervalMS)).ToString();
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
        }

        internal void PaintGraphics(System.Drawing.Graphics gr)
        {
            SetModeFromOutputLatch(outputLatch?.Value ?? 0);

            gr.InterpolationMode = InterpolationMode.NearestNeighbor;

            bool isGraphicsMode = GraphicsMode == GraphicsMode.Graphics;

            if (isGraphicsMode)
                    graphicsModeRenderer?.Render(gr, BackgroundColour == BackgroundColour.Orange);
            else
               textModeRenderer?.Render(gr, BackgroundColour == BackgroundColour.Orange);
        }
    }
}
