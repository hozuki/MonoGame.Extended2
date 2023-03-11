using System;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.WinForms;

public sealed class TimedEventArgs : EventArgs
{

    public TimedEventArgs(GameTime gameTime)
    {
        Guard.ArgumentNotNull(gameTime, nameof(gameTime));

        GameTime = gameTime;
    }

    public GameTime GameTime { get; }

}
