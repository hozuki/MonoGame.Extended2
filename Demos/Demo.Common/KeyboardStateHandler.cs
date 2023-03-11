using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Demo;

public sealed class KeyboardStateHandler : GameComponent
{

    public KeyboardStateHandler(Game game)
        : base(game)
    {
        _previousState = Keyboard.GetState();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (!Enabled)
        {
            return;
        }

        var state = Keyboard.GetState();

        var oldPressed = _previousState.GetPressedKeys();
        var newPressed = state.GetPressedKeys();

        foreach (var key in oldPressed)
        {
            if (newPressed.Contains(key))
            {
                continue;
            }

            var e = new KeyEventArgs(key, KeyState.Down, KeyState.Up);

            KeyUp?.Invoke(this, e);
        }

        foreach (var key in newPressed)
        {
            if (oldPressed.Contains(key))
            {
                continue;
            }

            var e = new KeyEventArgs(key, KeyState.Up, KeyState.Down);

            KeyDown?.Invoke(this, e);
        }

        _previousState = state;
    }

    public event EventHandler<KeyEventArgs>? KeyDown;

    public event EventHandler<KeyEventArgs>? KeyUp;

    private KeyboardState _previousState;

}
