using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace VzEmulator.Screen
{
    internal class SearchResultsAnnotator : IAnnotator
    {
        public HashSet<int> SearchResults = new HashSet<int>();

        public void PostRender(int x, int y, int offset, int scale, byte c, Rectangle destRect, Graphics gr)
        {
            //Highlight search results

            if (SearchResults.Contains(offset))
            {
                var borderColor = Color.FromArgb(128, 255, 255, 50);
                var highlightColor = Color.FromArgb(128, 255, 255, 200);
                Brush bgBrush = new SolidBrush(highlightColor);
                Pen borderPen = new Pen(borderColor, 1f);
                var fillRect = new Rectangle(destRect.Left, destRect.Top, 8 * scale, 12 * scale);
                var borderRect = new Rectangle(destRect.Left, destRect.Top, 7 * scale, 11 * scale);
                gr.FillRectangle(bgBrush, fillRect);
                gr.DrawRectangle(borderPen, borderRect);
            }
        }
    }
}
