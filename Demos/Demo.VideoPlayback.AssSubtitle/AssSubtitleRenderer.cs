using System;
using Assassin;
using Assassin.Native;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Framework.Media;

namespace Demo.VideoPlayback.AssSubtitle;

public class AssSubtitleRenderer : SubtitleRenderer
{

    public AssSubtitleRenderer()
    {
        _library = AssLibrary.Create();
        _renderer = _library.CreateRenderer();

        _renderer.SetFonts("Arial");
    }

    public void LoadFromFile(string fileName)
    {
        _currentTrack?.Dispose();

        var source = new FileAssSource(fileName);

        _currentTrack = _library.CreateTrack(source);
    }

    public void LoadFromString(string content)
    {
        _currentTrack?.Dispose();

        var source = new StringAssSource(content);

        _currentTrack = _library.CreateTrack(source);
    }

    public override Point Dimensions
    {
        get => _dimensions;
        set
        {
            _dimensions = value;

            _renderer.SetFrameSize(value.X, value.Y);
        }
    }

    public override void Render(TimeSpan time, RenderTarget2D texture)
    {
        if (!Enabled)
        {
            return;
        }

        if (_currentTrack == null)
        {
            // Subtitle is not loaded
            return;
        }

        var now = (long)Math.Round(time.TotalMilliseconds);
        var image = _renderer.RenderFrame(_currentTrack, now);

        var width = texture.Width;
        var height = texture.Height;

        if (_textureDataBuffer is null || _textureDataBuffer.Length == width * height)
        {
            _textureDataBuffer = new Color32Rgba[width * height];
        }

        texture.GetData(_textureDataBuffer);

        unsafe
        {
            fixed (Color32Rgba* buffer = _textureDataBuffer)
            {
                image.Blend(buffer, width, height);
            }
        }

        texture.SetData(_textureDataBuffer);
    }

    protected override void Dispose(bool disposing)
    {
        _textureDataBuffer = null;

        if (disposing)
        {
            _currentTrack?.Dispose();
            _currentTrack = null;

            _renderer.Dispose();
            _library.Dispose();
        }
    }

    private readonly AssLibrary _library;

    private readonly AssRenderer _renderer;

    private AssTrack? _currentTrack;

    private Color32Rgba[]? _textureDataBuffer;

    private Point _dimensions;

}
