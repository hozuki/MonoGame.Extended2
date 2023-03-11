using System.Runtime.CompilerServices;

namespace MonoGame.Extended.Text;

internal static class FontHelper
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float PointsToPixels(float points)
    {
        return points * 4 / 3;
    }

}
