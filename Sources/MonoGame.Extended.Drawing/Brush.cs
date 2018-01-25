using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EffectLoaderFunc = System.Func<Microsoft.Xna.Framework.Graphics.GraphicsDevice, (Microsoft.Xna.Framework.Graphics.Effect Effect, bool IsShared)>;

namespace MonoGame.Extended.Drawing {
    public abstract class Brush : DisposableBase {

        protected Brush([NotNull] DrawingContext context, [NotNull] EffectLoaderFunc effectLoaderFunc, BrushProperties brushProperties) {
            Guard.EnsureArgumentNotNull(context, nameof(context));
            Guard.EnsureArgumentNotNull(effectLoaderFunc, nameof(effectLoaderFunc));

            DrawingContext = context;
            BrushProperties = brushProperties;

            (_brushEffect, _isEffectShared) = effectLoaderFunc(context.GraphicsDevice);
        }

        public BrushProperties BrushProperties { get; }

        internal void Render([NotNull] Triangle[] triangles) {
            RenderInternal(triangles, _brushEffect);
        }

        internal DrawingContext DrawingContext { get; }

        protected abstract void RenderInternal([NotNull] Triangle[] triangles, [NotNull] Effect effect);

        protected override void Dispose(bool disposing) {
            if (!_isEffectShared) {
                _brushEffect?.Dispose();
            }

            _brushEffect = null;
        }

        private Effect _brushEffect;
        private readonly bool _isEffectShared;

    }
}
