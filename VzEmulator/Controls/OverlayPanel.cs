using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VzEmulatorControls
{
    internal class OverlayPanel : OverlayPanelBase
    {
        internal List<(string, string)> TextElements { get; } = new List<(string, string)>();

        public void Draw()
        {
            OnDraw();
        }
        protected override void OnDraw()
        {
            if (Visible)
            {
                if (TextElements is null) return;

                var leftMargin = 0;
                var topMargin = 0;
                var ySpacing = 3;
                var yPosition = topMargin;

                foreach ((string, string) element in TextElements)
                {
                    string key, value;
                    (key, value) = element;

                    float fontSize = 12f;
                    Point textPosition = new Point(leftMargin, yPosition);
                    var textRect = DrawText($"{key} {value}", "Microsoft Sans Serif", fontSize
                        , FontStyle.Bold, Brushes.Blue, textPosition);

                    yPosition = textRect.Bottom + ySpacing; //Position next line under the rectangle of the text just drawn
                }
            }
        }
 
    }
}
