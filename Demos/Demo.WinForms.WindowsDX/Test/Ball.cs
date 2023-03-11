using System;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework;

namespace Demo.WinForms.WindowsDX.Test;

[SupportedOSPlatform("windows7.0")]
internal class Ball
{

    private static readonly Random Rand = new Random();

    public float Radius { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Direction { get; set; }

    public void Randomize(Rectangle viewport)
    {
        Position = new Vector2(
            (float)Rand.NextDouble() * (viewport.Width - Radius * 2) + Radius,
            (float)Rand.NextDouble() * (viewport.Height - Radius * 2) + Radius);
        Direction = new Vector2((float)Rand.NextDouble(), (float)Rand.NextDouble());
        Direction.Normalize();
        Direction *= ((float)Rand.NextDouble() * 240 + 60);
    }

    public Rectangle Bounds => new((int)(Position.X - Radius), (int)(Position.Y - Radius), (int)(Radius * 2), (int)(Radius * 2));

}
