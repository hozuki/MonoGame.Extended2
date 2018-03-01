using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGame.Extended.WinForms.Input {
    public static class ControlKeyboard {

        public static KeyboardState GetState() {
            var pressedKeys = _currentKeys;

            var numLock = Array.IndexOf(pressedKeys, Keys.NumLock) >= 0;
            var capsLock = Array.IndexOf(pressedKeys, Keys.CapsLock) >= 0;

            return new KeyboardState(pressedKeys, capsLock, numLock);
        }

        [Obsolete]
        public static KeyboardState GetState(PlayerIndex playerIndex) {
            return GetState();
        }

        internal static void SetKeys([NotNull] List<Keys> keys) {
            Guard.ArgumentNotNull(keys, nameof(keys));

            if (!ArrayCache.TryGetValue(keys.Count, out _currentKeys)) {
                _currentKeys = new Keys[keys.Count];
                ArrayCache.Add(keys.Count, _currentKeys);
            }

            keys.CopyTo(_currentKeys);
        }

        private static Keys[] _currentKeys = new Keys[0];
        private static readonly Dictionary<int, Keys[]> ArrayCache = new Dictionary<int, Keys[]>();

    }
}
