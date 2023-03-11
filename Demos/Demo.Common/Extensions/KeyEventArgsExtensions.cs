using Microsoft.Xna.Framework.Input;

namespace Demo.Extensions;

public static class KeyEventArgsExtensions
{

    public static bool IsPressed(this KeyEventArgs e)
    {
        return e is { OldState: KeyState.Up, NewState: KeyState.Down };
    }

    public static bool IsReleased(this KeyEventArgs e)
    {
        return e is { OldState: KeyState.Down, NewState: KeyState.Up };
    }

}
