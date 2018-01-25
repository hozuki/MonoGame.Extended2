using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Xna.Framework {
    public struct RectangleF {

        public RectangleF(float left, float top, float width, float height) {
            _left = left;
            _top = top;
            _width = width;
            _height = height;
        }

        public float Left {
            get => _left;
            set => _left = value;
        }

        public float X {
            get => _left;
            set => _left = value;
        }

        public float Top {
            get => _top;
            set => _top = value;
        }

        public float Y {
            get => _top;
            set => _top = value;
        }

        public float Width {
            get => _width;
            set => _width = value;
        }

        public float Height {
            get => _height;
            set => _height = value;
        }

        public float Right {
            get => _left + _width;
            set => _width = value - Left;
        }

        public float Bottom {
            get => _top + _height;
            set => _height = value - _top;
        }

        public bool IsEmpty => Width.Equals(0) || Height.Equals(0);

        // ReSharper disable once InconsistentNaming
        public static RectangleF FromLTRB(float left, float top, float right, float bottom) {
            var width = right - left;
            var height = bottom - top;

            return new RectangleF(left, top, width, height);
        }

        public static RectangleF Union(RectangleF rect1, RectangleF rect2) {
            var left = Math.Min(rect1.Left, rect2.Left);
            var top = Math.Min(rect1.Top, rect2.Top);
            var right = Math.Max(rect1.Right, rect2.Right);
            var bottom = Math.Max(rect1.Bottom, rect2.Bottom);

            return FromLTRB(left, right, top, bottom);
        }

        public static RectangleF Intersect(RectangleF value1, RectangleF value2) {
            var left = value1.X > value2.X ? value1.X : value2.X;
            var top = value1.Y > value2.Y ? value1.Y : value2.Y;
            var right = value1.Right < value2.Right ? value1.Right : value2.Right;
            var bottom = value1.Bottom < value2.Bottom ? value1.Bottom : value2.Bottom;

            if (right > left && bottom > top) {
                return FromLTRB(left, right, top, bottom);
            } else {
                return EmptyRectangle;
            }
        }

        public bool Intersects(RectangleF other) {
            return Left < other.Right && other.Left < Right && Top < other.Bottom && other.Top < Bottom;
        }

        public static implicit operator RectangleF(Rectangle rect) {
            return new RectangleF(rect.Left, rect.Top, rect.Width, rect.Bottom);
        }

        public static RectangleF Empty => EmptyRectangle;

        private static readonly RectangleF EmptyRectangle = new RectangleF();

        private float _left, _top, _width, _height;

    }
}