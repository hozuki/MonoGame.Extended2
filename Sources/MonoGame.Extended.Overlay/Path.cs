using Microsoft.Xna.Framework;
using SkiaSharp;

namespace MonoGame.Extended.Overlay;

public sealed class Path : DisposableBase
{

    public Path()
    {
        _path = new SKPath();
    }

    public void AddCircle(Vector2 center, float radius)
    {
        AddCircle(center.X, center.Y, radius);
    }

    public void AddCircle(float x, float y, float radius)
    {
        _path.AddCircle(x, y, radius);
    }

    public void AddEllipse(Rectangle rect)
    {
        AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void AddEllipse(float x, float y, float width, float height)
    {
        var rect = new SKRect(x, y, x + width, y + height);
        _path.AddOval(rect);
    }

    public void AddRectangle(Rectangle rect)
    {
        AddRectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void AddRectangle(float x, float y, float width, float height)
    {
        var rect = new SKRect(x, y, x + width, y + height);
        _path.AddRect(rect);
    }

    public void CombineWith(Path otherPath, Vector2 offset)
    {
        CombineWith(otherPath, offset.X, offset.Y);
    }

    public void CombineWith(Path otherPath, float offsetX, float offsetY)
    {
        _path.AddPath(otherPath.NativePath, offsetX, offsetY);
    }

    public void Close()
    {
        _path.Close();
    }

    internal SKPath NativePath => _path;

    protected override void Dispose(bool disposing)
    {
        _path.Dispose();
    }

    private readonly SKPath _path;

}
