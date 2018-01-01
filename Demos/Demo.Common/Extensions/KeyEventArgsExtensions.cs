using Microsoft.Xna.Framework.Input;

namespace Demo.Extensions {
    public static class KeyEventArgsExtensions {

        public static bool IsPressed(this KeyEventArgs e) {
            return e.OldState == KeyState.Up && e.NewState == KeyState.Down;
        }

        public static bool IsReleased(this KeyEventArgs e) {
            return e.OldState == KeyState.Down && e.NewState == KeyState.Up;
        }

    }
}
