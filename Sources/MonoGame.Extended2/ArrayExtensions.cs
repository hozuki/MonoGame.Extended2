using System;
using System.Runtime.CompilerServices;

namespace MonoGame.Extended;

public static class ArrayExtensions
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this T[] array, T? element)
    {
        return Array.IndexOf(array, element) >= 0;
    }

}
