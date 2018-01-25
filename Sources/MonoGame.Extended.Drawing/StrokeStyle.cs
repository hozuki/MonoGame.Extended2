using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing {
    public sealed class StrokeStyle {

        public StrokeStyle(CapStyle dashCap = CapStyle.Flat, [CanBeNull] float[] dashes = null, float dashOffset = 0, DashStyle dashStyle = DashStyle.Solid, CapStyle endCap = CapStyle.Flat, LineJoin lineJoin = LineJoin.Round, float miterLimit = 0, CapStyle startCap = CapStyle.Flat) {
            DashCap = dashCap;
            Dashes = dashes;
            DashOffset = dashOffset;
            DashStyle = dashStyle;
            EndCap = endCap;
            LineJoin = lineJoin;
            MiterLimit = miterLimit;
            StartCap = startCap;
        }

        public CapStyle DashCap { get; }

        public float[] Dashes { get; }

        public float DashOffset { get; }

        public DashStyle DashStyle { get; }

        public CapStyle EndCap { get; }

        public LineJoin LineJoin { get; }

        public float MiterLimit { get; }

        public CapStyle StartCap { get; }

        internal static readonly StrokeStyle DefaultStyle = new StrokeStyle();

    }
}
