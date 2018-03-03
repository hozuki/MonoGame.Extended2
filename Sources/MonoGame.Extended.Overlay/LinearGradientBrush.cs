using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Overlay.Extensions;
using SkiaSharp;

namespace MonoGame.Extended.Overlay {
    public sealed class LinearGradientBrush : Brush {

        public LinearGradientBrush()
            : this(Vector2.Zero, Vector2.One, Color.Black, Color.White) {
        }

        public LinearGradientBrush(Point startPoint, Point endPoint, Color startColor, Color endColor)
            : this(startPoint.ToVector2(), endPoint.ToVector2(), startColor, endColor) {
        }

        public LinearGradientBrush(Vector2 startPoint, Vector2 endPoint, Color startColor, Color endColor) {
            _startPoint = startPoint;
            _endPoint = endPoint;

            _interpolationColors = CreateColorBlend(startColor, endColor);
            _arePropertiesDirty = true;

            RecreatePaint();
        }

        [NotNull]
        public ColorBlend InterpolationColors {
            get => _interpolationColors;
            set {
                Guard.ArgumentNotNull(value, nameof(value));

                _interpolationColors = value;
                _arePropertiesDirty = true;
            }
        }

        [NotNull]
        public Color[] LinearColors {
            get {
                var colors = _interpolationColors.Colors;

                if (colors.Length != 2) {
                    colors = new[] {
                        colors[0],
                        colors[colors.Length - 1]
                    };
                    _interpolationColors = CreateColorBlend(colors);
                }

                return colors;
            }
            set {
                _interpolationColors = CreateColorBlend(value);
                _arePropertiesDirty = true;
            }
        }

        public Vector2 StartPoint {
            get => _startPoint;
            set {
                _startPoint = value;
                _arePropertiesDirty = true;
            }
        }

        public Vector2 EndPoint {
            get => _endPoint;
            set {
                _endPoint = value;
                _arePropertiesDirty = true;
            }
        }

        public Matrix Transform {
            get => _transform;
            set {
                _transform = value;
                _arePropertiesDirty = true;
            }
        }

        public TileMode TileMode {
            get => _tileMode;
            set {
                _tileMode = value;
                _arePropertiesDirty = true;
            }
        }

        internal override SKPaint Paint {
            get {
                if (_arePropertiesDirty) {
                    RecreatePaint();
                }

                return _paint;
            }
        }

        protected override void Dispose(bool disposing) {
            // Strange... if there is no "disposing" check, this call will throw an AccessViolationException.
            if (disposing) {
                Paint.Shader?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void RecreatePaint() {
            if (!_arePropertiesDirty) {
                return;
            }

            _paint?.Shader?.Dispose();
            _paint?.Dispose();

            var paint = new SKPaint();

            var localMatrix = new SKMatrix();
            var transform = _transform;

            var matrixValues = new[] {
                transform.M11, transform.M21, transform.M31,
                transform.M12, transform.M22, transform.M32,
                transform.M13, transform.M23, transform.M33
            };

            localMatrix.Values = matrixValues;

            var start = new SKPoint(_startPoint.X, _startPoint.Y);
            var end = new SKPoint(_endPoint.X, _endPoint.Y);

            var colors = Array.ConvertAll(_interpolationColors.Colors, XnaExtensions.ToSKColor);
            var positions = _interpolationColors.Positions;

            if (colors.Length != positions.Length) {
                var minLength = Math.Min(colors.Length, positions.Length);

                if (colors.Length != minLength) {
                    var newColors = new SKColor[minLength];

                    Array.Copy(colors, newColors, minLength);
                    colors = newColors;
                }

                if (positions.Length != minLength) {
                    var newPositions = new float[minLength];

                    Array.Copy(positions, newPositions, minLength);
                    positions = newPositions;
                }
            }

            var linearShader = SKShader.CreateLinearGradient(start, end, colors, positions, (SKShaderTileMode)TileMode, localMatrix);

            paint.Shader = linearShader;
            paint.IsAntialias = true;
            paint.IsStroke = false;

            _paint = paint;

            _arePropertiesDirty = false;
        }

        private static ColorBlend CreateColorBlend(Color startColor, Color endColor) {
            var colorBlend = new ColorBlend(2);

            colorBlend.Colors[0] = startColor;
            colorBlend.Colors[1] = endColor;
            colorBlend.Positions[0] = 0;
            colorBlend.Positions[1] = 1;

            return colorBlend;
        }

        private static ColorBlend CreateColorBlend([NotNull] Color[] linearColors) {
            Guard.NotNull(linearColors, nameof(linearColors));

            if (linearColors.Length != 2) {
                throw new ArgumentException("The color array must contain 2 colors.");
            }

            return CreateColorBlend(linearColors[0], linearColors[1]);
        }

        [NotNull]
        private ColorBlend _interpolationColors;

        private Vector2 _startPoint;
        private Vector2 _endPoint;

        private Matrix _transform = Matrix.Identity;

        private TileMode _tileMode = TileMode.Clamp;

        private bool _arePropertiesDirty;

        private SKPaint _paint;

    }
}
