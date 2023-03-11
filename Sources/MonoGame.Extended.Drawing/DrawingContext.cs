using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Drawing.Geometries;

namespace MonoGame.Extended.Drawing;

/// <summary>
/// <see cref="DrawingContext"/> is responsible for organizing drawing objects, such as <see cref="Brush"/> and <see cref="Pen"/>.
/// </summary>
[PublicAPI]
public sealed class DrawingContext : DisposableBase
{

    public DrawingContext(GraphicsDevice graphicsDevice, GraphicsBackend backend)
    {
        GraphicsDevice = graphicsDevice;
        Backend = backend;
        EffectResources = new DrawingContextEffectResources(this);

        _currentTransform = Matrix3x2.Identity;
        _transforms = new Stack<Matrix3x2>();

        graphicsDevice.DeviceReset += GraphicsDevice_DeviceReset;
        UpdateProjectionMatrix();
    }

    public GraphicsDevice GraphicsDevice { get; }

    public GraphicsBackend Backend { get; }

    public void DrawEllipse(Pen pen, Ellipse ellipse)
    {
        var ellipseGeo = new EllipseGeometry(ellipse);

        DrawGeometry(pen, ellipseGeo);
    }

    public void DrawGeometry(Pen pen, Geometry geometry)
    {
        var mesh = new Mesh();
        var tessSink = mesh.Open();

        geometry.TessellateOutline(pen.StrokeWidth, pen.StrokeStyle, null, Geometry.DefaultFlatteningTolerance, tessSink);

        FillMesh(pen.Brush, mesh);
    }

    public void DrawLine(Pen pen, Vector2 point1, Vector2 point2)
    {
        var lineGeo = new LineGeometry(point1, point2);

        DrawGeometry(pen, lineGeo);
    }

    public void DrawRectangle(Pen pen, RectangleF rectangle)
    {
        var rectGeo = new RectangleGeometry(rectangle);

        DrawGeometry(pen, rectGeo);
    }

    public void DrawRoundedRectangle(Pen pen, RoundedRectangle roundedRectangle)
    {
        var roundedRectGeo = new RoundedRectangleGeometry(roundedRectangle);

        DrawGeometry(pen, roundedRectGeo);
    }

    public void FillEllipse(Brush brush, Ellipse ellipse)
    {
        var ellipseGeo = new EllipseGeometry(ellipse);

        FillGeometry(brush, ellipseGeo);
    }

    public void FillGeometry(Brush brush, Geometry geometry)
    {
        var triangles = geometry.TessellateForFillGeometry();

        brush.Render(triangles, _currentTransform);
    }

    public void FillMesh(Brush brush, Mesh mesh)
    {
        Guard.ArgumentNotNull(brush, nameof(brush));

        if (mesh.Triangles == null)
        {
            return;
        }

        brush.Render(mesh.Triangles, _currentTransform);
    }

    public void FillRectangle(Brush brush, RectangleF rectangle)
    {
        var rectGeometry = new RectangleGeometry(rectangle);

        FillGeometry(brush, rectGeometry);
    }

    public void FillRoundedRectangle(Brush brush, RoundedRectangle roundedRectangle)
    {
        var roundedRectGeo = new RoundedRectangleGeometry(roundedRectangle);

        FillGeometry(brush, roundedRectGeo);
    }

    #region Transforms

    public void SetCurrentTransform(Matrix3x2 transform)
    {
        _currentTransform = transform;
    }

    public void Translate(float x, float y)
    {
        _currentTransform *= Matrix3x2.CreateTranslation(x, y);
    }

    public void Translate(Vector2 translation)
    {
        _currentTransform *= Matrix3x2.CreateTranslation(translation);
    }

    public void PushTransform()
    {
        _transforms.Push(_currentTransform);
    }

    public Matrix3x2 PopTransform()
    {
        if (_transforms.Count == 0)
        {
            throw new InvalidOperationException("No pushed transforms in stack.");
        }

        var popped = _transforms.Pop();
        _currentTransform = popped;

        return popped;
    }

    #endregion

    internal DrawingContextEffectResources EffectResources { get; }

    /// <summary>
    /// This orthographic projection matrix has a NEGATIVE Y position (i.e. y to 0), so it
    /// helps drawing elements to move to the fourth quadrant.
    /// </summary>
    internal Matrix DefaultOrthographicProjection { get; private set; }

    protected override void Dispose(bool disposing)
    {
        GraphicsDevice.DeviceReset -= GraphicsDevice_DeviceReset;
    }

    private void UpdateProjectionMatrix()
    {
        var viewport = GraphicsDevice.Viewport;

        DefaultOrthographicProjection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0.5f, 10f);
    }

    private void GraphicsDevice_DeviceReset(object? sender, EventArgs e)
    {
        UpdateProjectionMatrix();
    }

    private Matrix3x2 _currentTransform;
    private readonly Stack<Matrix3x2> _transforms;

}
