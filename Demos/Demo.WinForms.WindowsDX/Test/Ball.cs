using System;
using Microsoft.Xna.Framework;

namespace Demo.WinForms.WindowsDX.Test {
    internal class Ball {
        private static Random _rand = new Random();

        public float Radius { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }

        public void Randomize(Rectangle viewport) {
            Position = new Vector2(
                (float)_rand.NextDouble() * (viewport.Width - Radius * 2) + Radius,
                (float)_rand.NextDouble() * (viewport.Height - Radius * 2) + Radius);
            Direction = new Vector2((float)_rand.NextDouble(), (float)_rand.NextDouble());
            Direction.Normalize();
            Direction *= ((float)_rand.NextDouble() * 240 + 60);
        }

        public Rectangle Bounds {
            get { return new Rectangle((int)(Position.X - Radius), (int)(Position.Y - Radius), (int)(Radius * 2), (int)(Radius * 2)); }
        }
    }
}
