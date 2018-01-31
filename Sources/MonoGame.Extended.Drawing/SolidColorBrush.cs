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
            var brushEffect = (SolidBrushEffect)effect;
            var graphicsDevice = DrawingContext.GraphicsDevice;
            var presentParams = graphicsDevice.PresentationParameters;
            var clientWidth = presentParams.BackBufferWidth;
            var clientHeight = presentParams.BackBufferHeight;
            var props = BrushProperties;

            // Move to the fourth quadrant.
            var projection = Matrix.CreateOrthographicOffCenter(0, clientWidth, -clientHeight, 0, 0.5f, 10f);
            //            var projection = Matrix.CreateOrthographic(clientWidth, clientHeight, 0.5f, 100f);

            brushEffect.SetWorldViewProjection(BrushEffect.DefaultWorld, BrushEffect.DefaultView, projection);
            brushEffect.SetOpacity(props.Opacity);

            graphicsDevice.RasterizerState = DefaultRasterizerState;
            graphicsDevice.DepthStencilState = DefaultDepthStencilState;

            brushEffect.Apply();

            var colorf = Color.ToVector4();
            var vertices = new Vertex[triangles.Length * 3];
            var indices = new uint[triangles.Length * 3];

            for (var i = 0; i < triangles.Length; ++i) {
                var s = i * 3;
                vertices[s] = new Vertex {
                    Position = Matrix3x2.Transform(props.Transform, triangles[i].Point1),
                    Color = colorf
                };
                vertices[s + 1] = new Vertex {
                    Position = Matrix3x2.Transform(props.Transform, triangles[i].Point2),
                    Color = colorf
                };
                vertices[s + 2] = new Vertex {
                    Position = Matrix3x2.Transform(props.Transform, triangles[i].Point3),
                    Color = colorf
                };
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
            if (_solidBrushEffect != null) {
                return (_solidBrushEffect, true);
            }

            _solidBrushEffect = SolidBrushEffect.Create(drawingContext);

            return (_solidBrushEffect, true);
        }

        private static readonly VertexElement[] VertexElements = {
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
        };

        private static Effect _solidBrushEffect;

        private static readonly RasterizerState DefaultRasterizerState = new RasterizerState {
            CullMode = CullMode.None,
            MultiSampleAntiAlias = true
        };

        private static readonly DepthStencilState DefaultDepthStencilState = new DepthStencilState {
            DepthBufferEnable = false,
            DepthBufferFunction = CompareFunction.Always,
            DepthBufferWriteEnable = true
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Vertex {

            internal Vector2 Position;

            internal Vector4 Color;

        }

    }
}
