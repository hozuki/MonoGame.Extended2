using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing;

[PublicAPI]
public enum CombineMode
{

    Union = 0,
    Intersect = 1,
    Xor = 2,
    Exclude = 3,

}
