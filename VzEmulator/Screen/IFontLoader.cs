using VzEmulate2;

namespace VzEmulator.Screen
{
    interface IFontLoader
    {
        DirectBitmap LoadFont(string fileName);
    }
}
