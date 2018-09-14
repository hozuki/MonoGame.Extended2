using JetBrains.Annotations;

namespace MonoGame.Extended.Overlay {
    public sealed class StringFormat {

        public bool IsVertical { get; set; }

        public TextAlign Align { get; set; } = TextAlign.Left;

        public VerticalTextAlign VerticalAlign { get; set; } = VerticalTextAlign.Top;

        [CanBeNull]
        public float? PreferredLineHeight { get; set; }

    }
}
