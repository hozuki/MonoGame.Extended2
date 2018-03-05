using JetBrains.Annotations;
using SkiaSharp;

namespace MonoGame.Extended.Overlay {
    internal interface IPaintProvider {

        [NotNull]
        SKPaint Paint { get; }

    }
}
