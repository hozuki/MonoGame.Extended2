using System.Collections.Generic;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Demo.WinForms.WindowsDX.Test;

[SupportedOSPlatform("windows7.0")]
public class Engine
{

    private const int PaddleWidth = 50;
    private const int PaddleHeight = 10;
    private const int PaddleBottomMargin = 20;
    private const int BallRadius = 5;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;

    private float _paddleX;
    private List<Ball> _balls;

    private Texture2D _whiteTex;

    public int BottomHitCount { get; private set; }

    public Engine(IGraphicsDeviceService graphics)
    {
        _graphicsDevice = graphics.GraphicsDevice;

        _spriteBatch = new SpriteBatch(_graphicsDevice);
    }

    public void Initialize()
    {
        _whiteTex = SolidColorTexture(_graphicsDevice, Color.White);

        var ball1 = new Ball
        {
            Radius = BallRadius
        };
        ball1.Randomize(_graphicsDevice.Viewport.Bounds);

        _balls = new List<Ball> { ball1 };
    }

    public void Update(GameTime gameTime)
    {
        UpdatePaddle(gameTime);

        foreach (var ball in _balls)
        {
            UpdateBall(ball, gameTime);
        }
    }

    public void Draw(GameTime gameTime)
    {
        _graphicsDevice.Clear(Color.Black);

        Rectangle paddleRect = new Rectangle((int)_paddleX, _graphicsDevice.Viewport.Height - PaddleBottomMargin, PaddleWidth, PaddleHeight);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_whiteTex, paddleRect, Color.White);

        foreach (var ball in _balls)
        {
            _spriteBatch.Draw(_whiteTex, ball.Bounds, Color.White);
        }

        _spriteBatch.End();
    }

    public int BallCount => _balls.Count;

    public void AddBall()
    {
        Ball ball = new Ball() { Radius = BallRadius };
        ball.Randomize(_graphicsDevice.Viewport.Bounds);

        _balls.Add(ball);
    }

    public void RemoveBall()
    {
        if (_balls.Count > 0)
        {
            _balls.RemoveAt(_balls.Count - 1);
        }
    }

    private static Texture2D SolidColorTexture(GraphicsDevice device, Color color)
    {
        var tex = new Texture2D(device, 1, 1);
        var data = new Color[1] { color };

        tex.SetData(data);
        return tex;
    }

    private void UpdatePaddle(GameTime gameTime)
    {
        var keyboard = InputManager.GetKeyboardState();
        if (keyboard.IsKeyDown(Keys.Left) && _paddleX > 0)
        {
            _paddleX -= (float)(300 * gameTime.ElapsedGameTime.TotalSeconds);
        }
        else if (keyboard.IsKeyDown(Keys.Right) && _paddleX < (_graphicsDevice.Viewport.Width - PaddleWidth))
        {
            _paddleX += (float)(300 * gameTime.ElapsedGameTime.TotalSeconds);
        }
    }

    private void UpdateBall(Ball ball, GameTime gameTime)
    {
        var newPosition = ball.Position + ball.Direction * (float)gameTime.ElapsedGameTime.TotalSeconds;
        var newDirection = ball.Direction;

        if (newPosition.X < _graphicsDevice.Viewport.X + ball.Radius)
        {
            newPosition = new Vector2(_graphicsDevice.Viewport.X + BallRadius, newPosition.Y);
            newDirection = new Vector2(newDirection.X * -1, newDirection.Y);
        }

        if (newPosition.X >= _graphicsDevice.Viewport.X + _graphicsDevice.Viewport.Width - ball.Radius)
        {
            newPosition = new Vector2(_graphicsDevice.Viewport.X + _graphicsDevice.Viewport.Width - ball.Radius, newPosition.Y);
            newDirection = new Vector2(newDirection.X * -1, newDirection.Y);
        }

        if (newPosition.Y < _graphicsDevice.Viewport.Y + ball.Radius)
        {
            newPosition = new Vector2(newPosition.X, _graphicsDevice.Viewport.Y + ball.Radius);
            newDirection = new Vector2(newDirection.X, newDirection.Y * -1);
        }

        if (newPosition.Y >= _graphicsDevice.Viewport.Y + _graphicsDevice.Viewport.Height - ball.Radius)
        {
            newPosition = new Vector2(newPosition.X, _graphicsDevice.Viewport.Y + _graphicsDevice.Viewport.Height - ball.Radius);
            newDirection = new Vector2(newDirection.X, newDirection.Y * -1);
            BottomHitCount++;
        }

        ball.Position = newPosition;
        ball.Direction = newDirection;

        var paddleRect = new Rectangle((int)_paddleX, _graphicsDevice.Viewport.Height - PaddleBottomMargin, PaddleWidth, PaddleHeight);
        if (paddleRect.Intersects(ball.Bounds))
        {
            Bounce(ball, gameTime);
        }
    }

    private static void Bounce(Ball ball, GameTime gameTime)
    {
        var newPosition = ball.Position - ball.Direction * (float)gameTime.ElapsedGameTime.TotalSeconds;
        var newDirection = new Vector2(ball.Direction.X, ball.Direction.Y * -1);

        ball.Position = newPosition;
        ball.Direction = newDirection;
    }

}
