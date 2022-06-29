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

            //TODO extended graphics
            //extendedGraphicsLatch.value
            //bits 0,1 are bank switching. Handled in VideoMemory.cs rather than here
            //bit 2 (4)  => 6847 GM0
            //bit 3 (8)  => 6847 GM1 (Held high in unmodified vz. For compatiblity with unmodified, this could be defaulted high) 
            //bit 4 (16) => 6847 GM2
            //bit 5 (32)=> /INT/EXT (0 for Internal chargen/Semigraphics 4, 1 for External chargen/Semigraphics 6)
            //bit 6,7 unused
            //
            // GM0,1,2 Mode
            //0,0,0 CG1 - 64x64 4 Colour. 3 scan lines per pixel
            //0,0,1 RG1 - 128x64 2 Colour
            //0,1,0 CG2 - 128x64 4 Colour
            //0,1,1 RG2 - 128x96 2 Colour. 2 scan lines per pixel
            //1,0,0 CG3 - 128x96 4 Colour
            //1,0,1 RG3 - 128x192 2 Colour. 1 scan line per pixe
            //1,1,0 CG6 - 128x192 4 Colour
            //1,1,1 RG6 - 256x192 2 Colour

            //shoud this change to draw one scan line at a time? And time the CPU and sound to the scanline?

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
