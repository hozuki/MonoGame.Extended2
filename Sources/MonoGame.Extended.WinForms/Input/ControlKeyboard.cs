using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGame.Extended.WinForms.Input;

public static class ControlKeyboard
{

    public static KeyboardState GetState()
    {
        var pressedKeys = _currentKeys;

        var numLock = pressedKeys.Contains(Keys.NumLock);
        var capsLock = pressedKeys.Contains(Keys.CapsLock);

        return new KeyboardState(pressedKeys, capsLock, numLock);
    }

    [Obsolete("There is no equivalent to " + nameof(Keyboard) + "." + nameof(Keyboard.GetState) + "(" + nameof(PlayerIndex) + ") in XNA. The playerIndex argument is always ignored.")]
    public static KeyboardState GetState(PlayerIndex playerIndex)
    {
        return GetState();
    }

    internal static void SetKeys(List<Keys> keys)
    {
        Guard.ArgumentNotNull(keys, nameof(keys));

        if (ArrayCache.TryGetValue(keys.Count, out var currentKeys))
        {
            _currentKeys = currentKeys;
        }
        else
        {
            _currentKeys = new Keys[keys.Count];
            ArrayCache.Add(keys.Count, _currentKeys);
        }

        keys.CopyTo(_currentKeys);
    }

    private static Keys[] _currentKeys = Array.Empty<Keys>();
    private static readonly Dictionary<int, Keys[]> ArrayCache = new();

}
