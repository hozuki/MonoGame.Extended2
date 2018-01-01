using System;
using Microsoft.Xna.Framework.Input;

namespace Demo {
    public sealed class KeyEventArgs : EventArgs {

        public KeyEventArgs(Keys keyCode, KeyState oldState, KeyState newState) {
            KeyCode = keyCode;
            OldState = oldState;
            NewState = newState;
        }

        public Keys KeyCode { get; }

        public KeyState OldState { get; }

        public KeyState NewState { get; }

    }
}
