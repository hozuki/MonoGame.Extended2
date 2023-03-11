using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Extended.Drawing.Effects;

internal static class EffectLoadingResultExtensions
{

    public static void Deconstruct(this EffectLoadingResult loadingResult, out Effect effect, out bool isShared)
    {
        effect = loadingResult.Effect;
        isShared = loadingResult.IsShared;
    }

}
