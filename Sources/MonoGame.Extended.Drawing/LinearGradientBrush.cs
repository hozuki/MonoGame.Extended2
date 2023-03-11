using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Effects;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public sealed class LinearGradientBrush : Brush
{

    public LinearGradientBrush(DrawingContext context, LinearGradientBrushProperties properties, GradientStopCollection gradientStopCollection)
        : this(context, properties, BrushProperties.Default, gradientStopCollection)
    {
    }

    public LinearGradientBrush(DrawingContext context, LinearGradientBrushProperties properties, BrushProperties brushProperties, GradientStopCollection gradientStopCollection)
        : base(context, LoadEffect, brushProperties)
    {
        GradientStopCollection = gradientStopCollection;
        Properties = properties;
    }

    public GradientStopCollection GradientStopCollection { get; }

    public LinearGradientBrushProperties Properties { get; }

    protected override void RenderInternal(Triangle[] triangles, Effect effect, Matrix3x2? transform)
    {
        var brushEffect = (LinearGradientBrushEffect)effect;
        var graphicsDevice = DrawingContext.GraphicsDevice;
        var brushProps = BrushProperties;
        var props = Properties;
        var gsc = GradientStopCollection;

        var projection = DrawingContext.DefaultOrthographicProjection;

        var world = transform?.ToMatrix4x4() ?? BrushEffect.DefaultWorld;
        brushEffect.SetWorldViewProjection(world, BrushEffect.DefaultView, projection);
        brushEffect.Opacity = brushProps.Opacity;
        brushEffect.SetGradientStops(gsc.GradientStopsDirect);
        brushEffect.Gamma = gsc.Gamma;
        brushEffect.ExtendMode = gsc.ExtendMode;
        brushEffect.StartPoint = props.StartPoint;
        brushEffect.EndPoint = props.EndPoint;

        graphicsDevice.RasterizerState = DefaultBrushRasterizerState;
        graphicsDevice.DepthStencilState = DefaultBrushDepthStencilState;

        brushEffect.Apply();

        var vertices = new Vector2[triangles.Length * 3];
        var indices = new uint[triangles.Length * 3];

        for (var i = 0; i < triangles.Length; ++i)
        {
            var s = i * 3;

            vertices[s] = Matrix3x2.Transform(brushProps.Transform, triangles[i].Point1);
            vertices[s + 1] = Matrix3x2.Transform(brushProps.Transform, triangles[i].Point2);
            vertices[s + 2] = Matrix3x2.Transform(brushProps.Transform, triangles[i].Point3);
        }

        for (var i = 0; i < indices.Length; ++i)
        {
            indices[i] = (uint)i;
        }

        using (var vertexBuffer = new VertexBuffer(graphicsDevice, new VertexDeclaration(VertexElements), vertices.Length, BufferUsage.WriteOnly))
        {
            using (var indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly))
            {
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

    private static EffectLoadingResult LoadEffect(DrawingContext drawingContext)
    {
        _linearGradientBrushEffect ??= LinearGradientBrushEffect.Create(drawingContext);

        return new EffectLoadingResult(_linearGradientBrushEffect, true);
    }

    private static readonly VertexElement[] VertexElements =
    {
        new(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
    };

    private static Effect? _linearGradientBrushEffect;

}
