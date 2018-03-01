using Microsoft.Xna.Framework.Input;

namespace Demo.WinForms.WindowsDX.Test {
    public static class InputManager {

        private static InputManagerImplementation _implementation;

        public static void Initialize(InputManagerImplementation implementation) {
            _implementation = implementation;
        }

        public static KeyboardState GetKeyboardState() {
            if (_implementation == null)
                return new KeyboardState();

            return _implementation.GetKeyboardState();
        }

    }
}
