using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VzEmulatorControls
{
    internal abstract class OverlayPanelBase : Panel
    {
        protected Graphics graphics;
        protected abstract void OnDraw();

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Do nothing
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Visible)
            {
                graphics = e.Graphics;

                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;

                OnDraw();
            }
        }

        protected Rectangle DrawText(string text, string fontFamily, float fontSize
        , FontStyle style, Brush color, Point position)
        {
            try
            {
                Font font = new Font(fontFamily, fontSize, style);

                SizeF textSizeF = graphics.MeasureString(text, font);
                int width = (int)Math.Ceiling(textSizeF.Width);
                int height = (int)Math.Ceiling(textSizeF.Height);
                Size textSize = new Size(width, height);
                Rectangle rectangle = new Rectangle(position, textSize);

                graphics.DrawString(text, font, color, rectangle);

                return rectangle;
            } catch
            {
                return new Rectangle(0,0,0,0);
            }
        }
    }
}
