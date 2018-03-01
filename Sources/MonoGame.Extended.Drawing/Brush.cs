using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EffectLoaderFunc = System.Func<MonoGame.Extended.Drawing.DrawingContext, (Microsoft.Xna.Framework.Graphics.Effect Effect, bool IsShared)>;

namespace MonoGame.Extended.Drawing {
    public abstract class Brush : DisposableBase {

        protected Brush([NotNull] DrawingContext context, [NotNull] EffectLoaderFunc effectLoaderFunc, BrushProperties brushProperties) {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(effectLoaderFunc, nameof(effectLoaderFunc));

            DrawingContext = context;
            BrushProperties = brushProperties;

            (_brushEffect, _isEffectShared) = effectLoaderFunc(context);
        }

        public BrushProperties BrushProperties { get; }

        internal void Render([NotNull] Triangle[] triangles, [CanBeNull] Matrix3x2? transform) {
            RenderInternal(triangles, _brushEffect, transform);
        }

        internal DrawingContext DrawingContext { get; }

        protected abstract void RenderInternal([NotNull] Triangle[] triangles, [NotNull] Effect effect, [CanBeNull] Matrix3x2? transform);

        protected override void Dispose(bool disposing) {
            if (!_isEffectShared) {
                _brushEffect?.Dispose();
            }

            _brushEffect = null;
        }

        protected static readonly RasterizerState DefaultBrushRasterizerState = new RasterizerState {
            CullMode = CullMode.None,
            MultiSampleAntiAlias = true,
        };

        protected static readonly DepthStencilState DefaultBrushDepthStencilState = new DepthStencilState {
            DepthBufferEnable = false,
            DepthBufferFunction = CompareFunction.Always,
            DepthBufferWriteEnable = true
        };

        private Effect _brushEffect;
        private readonly bool _isEffectShared;

    }
}
