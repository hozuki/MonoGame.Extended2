using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// ReSharper disable once CheckNamespace
namespace MonoGame.Extended.Framework.Media {
    /// <summary>
    /// Represents a subtitle renderer interface.
    /// </summary>
    [PublicAPI]
    public interface ISubtitleRenderer : IDisposable {

        /// <summary>
        /// Gets or sets whether this renderer is enabled.
        /// </summary>
        [PublicAPI]
        bool Enabled { get; }

        /// <summary>
        /// Gets or sets the dimensions (expected width and height) of this renderer.
        /// </summary>
        [PublicAPI]
        Point Dimensions { set; }

        /// <summary>
        /// Renders the subtitle to the specified texture.
        /// </summary>
        /// <param name="time">The time when the corresponding subtitle should be rendered.</param>
        /// <param name="texture">The texture to render to.</param>
        [PublicAPI]
        void Render(TimeSpan time, [NotNull] RenderTarget2D texture);

    }
}
