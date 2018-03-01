using Demo.WinForms.WindowsDX.Test;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.WinForms.Input;

namespace Demo.WinForms.WindowsDX {
    internal class ControlInputManager : InputManagerImplementation {

        public override KeyboardState GetKeyboardState() {
            return ControlKeyboard.GetState();
        }

    }
}
