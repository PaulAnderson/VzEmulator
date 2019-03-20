using System.Drawing;

namespace VzEmulator.Screen
{
    internal class PngFontLoader : IFontLoader
    {

        public DirectBitmap LoadFont(string fileName)
        {
            Image loadedFont = Image.FromFile(fileName);
            DirectBitmap _FontBitmap = new DirectBitmap(loadedFont.Width, loadedFont.Height );
            var gr = Graphics.FromImage(_FontBitmap.Bitmap);
            gr.DrawImage(loadedFont, new Point(0,0));

            return _FontBitmap;
        }

    }
}
