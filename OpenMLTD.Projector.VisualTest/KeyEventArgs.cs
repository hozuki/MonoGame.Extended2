using System;
using Microsoft.Xna.Framework.Input;

namespace OpenMLTD.Projector.VisualTest {
    internal sealed class KeyEventArgs : EventArgs {

        internal KeyEventArgs(Keys keyCode, KeyState oldState, KeyState newState) {
            KeyCode = keyCode;
            OldState = oldState;
            NewState = newState;
        }

        internal Keys KeyCode { get; }

        internal KeyState OldState { get; }

        internal KeyState NewState { get; }

        internal bool IsPressed => OldState == KeyState.Up && NewState == KeyState.Up;

        internal bool IsReleased => OldState == KeyState.Down && NewState == KeyState.Up;

    }
}
