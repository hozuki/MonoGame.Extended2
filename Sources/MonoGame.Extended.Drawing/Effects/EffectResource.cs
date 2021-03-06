﻿using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing.Effects {
    internal sealed partial class EffectResource {

        private EffectResource([NotNull] string effectResourceName) {
            ResourceName = effectResourceName;
        }

        internal static EffectResource CreateSolidColorBrushEffectResource([NotNull] DrawingContext drawingContext) {
            return CreateEffectResource(drawingContext.Backend, SolidColorBrushResourceNames);
        }

        internal static EffectResource CreateLinearGradientBrushEffectResource([NotNull] DrawingContext drawingContext) {
            return CreateEffectResource(drawingContext.Backend, LinearGradientBrushResourceNames);
        }

        internal string ResourceName { get; }

        internal byte[] Bytecode => _bytecode ?? (_bytecode = Load(ResourceName));

        private static EffectResource CreateEffectResource(GraphicsBackend backend, [NotNull] IReadOnlyDictionary<GraphicsBackend, string> resourceMap) {
            var resourceName = resourceMap[backend];
            return new EffectResource(resourceName);
        }

        private static byte[] Load([NotNull] string effectResourceName) {
            Guard.NotNullOrEmpty(effectResourceName, nameof(effectResourceName));

            var assembly = Assembly.GetAssembly(typeof(EffectResource));

            return ReflectionHelper.LoadResource(assembly, effectResourceName);
        }

        private byte[] _bytecode;

    }
}
