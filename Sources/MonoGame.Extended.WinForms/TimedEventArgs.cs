using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.WinForms {
    public sealed class TimedEventArgs : EventArgs {

        public TimedEventArgs([NotNull] GameTime gameTime) {
            Guard.ArgumentNotNull(gameTime, nameof(gameTime));

            GameTime = gameTime;
        }

        [NotNull]
        public GameTime GameTime { get; }

    }
}
