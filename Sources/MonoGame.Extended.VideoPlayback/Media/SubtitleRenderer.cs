using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// ReSharper disable once CheckNamespace
namespace MonoGame.Extended.Framework.Media;

/// <inheritdoc cref="DisposableBase"/>
/// <inheritdoc cref="ISubtitleRenderer"/>
/// <summary>
/// A base implementation of <see cref="ISubtitleRenderer" />
/// </summary>
public abstract class SubtitleRenderer : DisposableBase, ISubtitleRenderer
{

    /// <inheritdoc/>
    public virtual bool Enabled { get; set; }

    /// <inheritdoc/>
    public virtual Point Dimensions { get; set; }

    /// <inheritdoc/>
    public abstract void Render(TimeSpan time, RenderTarget2D texture);

}
