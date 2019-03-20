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
            paintControl = ctrl;
            this.outputLatch = outputLatch;
            textModeRenderer = new TextModeRenderer(VideoMemory, 0);
            graphicsModeRenderer = new GraphicsModeRenderer(VideoMemory, 0);

            screenRefreshTimer = new Timer();
            screenRefreshTimer.Interval = RefreshIntervalMS;
            screenRefreshTimer.Tick += (v, a) => RenderScreen();
            screenRefreshTimer.Start();

            fpsCalculateTimer = new Timer();
            fpsCalculateTimer.Interval = 1000; //ms
            fpsCalculateTimer.Tick += (v, a) =>
            {
                CurrentFps = (fps / (fpsCalculateTimer.Interval / 1000.0)).ToString();
                fps = 0;
            };
            fpsCalculateTimer.Start();

            
            ctrl.Paint += new System.Windows.Forms.PaintEventHandler(Control_Paint);

        }

        private void SetModeFromOutputLatch(byte OutputLatchValue)
        {
            GraphicsMode = (OutputLatchValue & 0x08) == 0x08 ? GraphicsMode.Graphics : GraphicsMode.Text;
            BackgroundColour = (OutputLatchValue & 0x10) == 0x10 ? BackgroundColour.Orange : BackgroundColour.Green;
        }

        private void RenderScreen()
        {
            SetModeFromOutputLatch(outputLatch?.Value ?? 0);

            //bitmap display, graphics and text modes
            paintControl.Invalidate();
            paintControl.Update();

            fps++;

        }

        private void Control_Paint(object sender, PaintEventArgs e)
        {

            var gr = e.Graphics;

            gr.InterpolationMode = InterpolationMode.NearestNeighbor;

            bool graphicsMode = GraphicsMode == GraphicsMode.Graphics;

            if (graphicsMode)
            {
                //graphics mode
                if (graphicsModeRenderer != null)
                {
                    graphicsModeRenderer.Render(gr, BackgroundColour == BackgroundColour.Orange);
                }
            }
            else
            {
                //text mode
                if (textModeRenderer != null)
                {
                    textModeRenderer.Render(gr, BackgroundColour == BackgroundColour.Orange);
                }
            }
        }

    }
}
