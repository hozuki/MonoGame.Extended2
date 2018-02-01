using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Effects;

namespace MonoGame.Extended.Drawing {
    public sealed class SolidColorBrush : Brush {

        public SolidColorBrush([NotNull] DrawingContext context, Color color)
            : this(context, color, BrushProperties.Default) {
        }

        public SolidColorBrush([NotNull] DrawingContext context, Color color, BrushProperties brushProperties)
            : base(context, LoadEffect, brushProperties) {
            Color = color;
        }

        public Color Color { get; }

        protected override void RenderInternal(Triangle[] triangles, Effect effect) {
            var brushEffect = (SolidColorBrushEffect)effect;
            var graphicsDevice = DrawingContext.GraphicsDevice;
            var props = BrushProperties;

            var projection = DrawingContext.DefaultOrthographicProjection;

            brushEffect.SetWorldViewProjection(BrushEffect.DefaultWorld, BrushEffect.DefaultView, projection);
            brushEffect.Opacity = props.Opacity;
            brushEffect.Color = Color.ToVector4();

            graphicsDevice.RasterizerState = DefaultBrushRasterizerState;
            graphicsDevice.DepthStencilState = DefaultBrushDepthStencilState;

            brushEffect.Apply();

            var vertices = new Vector2[triangles.Length * 3];
            var indices = new uint[triangles.Length * 3];

            for (var i = 0; i < triangles.Length; ++i) {
                var s = i * 3;

                vertices[s] = Matrix3x2.Transform(props.Transform, triangles[i].Point1);
                vertices[s + 1] = Matrix3x2.Transform(props.Transform, triangles[i].Point2);
                vertices[s + 2] = Matrix3x2.Transform(props.Transform, triangles[i].Point3);
            }

            for (var i = 0; i < indices.Length; ++i) {
                indices[i] = (uint)i;
            }

            using (var vertexBuffer = new VertexBuffer(graphicsDevice, new VertexDeclaration(VertexElements), vertices.Length, BufferUsage.WriteOnly)) {
                using (var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly)) {
                    vertexBuffer.SetData(vertices);
                    indexBuffer.SetData(indices);

                    graphicsDevice.SetVertexBuffer(vertexBuffer);
                    graphicsDevice.Indices = indexBuffer;

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, triangles.Length);

                    graphicsDevice.SetVertexBuffer(null);
                    graphicsDevice.Indices = null;
                }
            }
        }

        private static (Effect Effect, bool IsShared) LoadEffect([NotNull] DrawingContext drawingContext) {
            if (_solidColorBrushEffect == null) {
                _solidColorBrushEffect = SolidColorBrushEffect.Create(drawingContext);
            }

            return (_solidColorBrushEffect, true);
        }

        private static readonly VertexElement[] VertexElements = {
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0)
        };

        private static Effect _solidColorBrushEffect;

    }
}
