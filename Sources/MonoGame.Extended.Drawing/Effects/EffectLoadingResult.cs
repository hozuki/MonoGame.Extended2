using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects;

internal sealed class EffectLoadingResult
{

    public EffectLoadingResult(Effect effect, bool isShared)
    {
        Effect = effect;
        IsShared = isShared;
    }

    public Effect Effect { get; }

    public bool IsShared { get; }

}
