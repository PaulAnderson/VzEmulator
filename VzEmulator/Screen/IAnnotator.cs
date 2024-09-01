using System.Drawing;

namespace VzEmulator.Screen
{
    public interface IAnnotator
    {
        void PostRender(int x, int y, int offset, int scale, byte c, Rectangle destRect, Graphics gr);
    }
}