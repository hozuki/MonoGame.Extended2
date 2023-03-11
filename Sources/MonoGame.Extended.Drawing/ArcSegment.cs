using JetBrains.Annotations;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Drawing;

// https://msdn.microsoft.com/en-us/library/windows/desktop/dd368065.aspx
[PublicAPI]
public struct ArcSegment
{

    public Vector2 Point { get; set; }

    public Vector2 Size { get; set; }

    public float RotationAngle { get; set; }

    public SweepDirection SweepDirection { get; set; }

    public ArcSize ArcSize { get; set; }

}
