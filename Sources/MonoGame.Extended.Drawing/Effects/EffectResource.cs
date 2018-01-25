using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing.Effects {
    internal sealed partial class EffectResource {

        private EffectResource([NotNull] string effectResourceName) {
            ResourceName = effectResourceName;
        }

        internal static EffectResource SolidBrush => CreateEffectResource(SolidBrushResourceNames);

        internal string ResourceName { get; }

        internal byte[] Bytecode => _bytecode ?? (_bytecode = Load(ResourceName));

        private static EffectResource CreateEffectResource(IReadOnlyDictionary<GraphicsBackend, string> resourceMap) {
            var resourceName = resourceMap[DrawingContext.GraphicsBackend];
            return new EffectResource(resourceName);
        }

        private static byte[] Load([NotNull] string effectResourceName) {
            Guard.EnsureArgumentNotNullOrEmpty(effectResourceName, nameof(effectResourceName));

            var assembly = Assembly.GetAssembly(typeof(EffectResource));

            return ReflectionHelper.LoadResource(assembly, effectResourceName);
        }

        private byte[] _bytecode;

    }
}
