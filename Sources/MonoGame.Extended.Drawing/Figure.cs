using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing {
    internal sealed class Figure {

        internal Figure(Vector2 origin, [NotNull] GeometryElement[] elements, FigureEnd figureEnd) {
            Origin = origin;
            Elements = elements;
            FigureEnd = figureEnd;
        }

        internal Vector2 Origin { get; }

        internal GeometryElement[] Elements { get; }

        internal FigureEnd FigureEnd { get; }

    }
}
