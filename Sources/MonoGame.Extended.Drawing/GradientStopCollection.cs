using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MonoGame.Extended.Drawing {
    public sealed class GradientStopCollection {

        public GradientStopCollection([NotNull] GradientStop[] gradientStops, Gamma gamma, ExtendMode extendMode) {
            if (gradientStops.Length < 2) {
                throw new ArgumentException("There must be at least 2 gradient stops.", nameof(gradientStops));
            }

            if (gradientStops.Length > MaximumGradientStops) {
                throw new ArgumentException($"GradientStopCollection supports maximum {MaximumGradientStops} gradient stops.", nameof(gradientStops));
            }

            var currentStopPos = gradientStops[0].Position;

            for (var i = 1; i < gradientStops.Length; ++i) {
                if (gradientStops[i].Position <= currentStopPos) {
                    throw new ArgumentException($"Position of gradient stop #{i} is less than its predecessor.");
                }

                currentStopPos = gradientStops[i].Position;
            }

            GradientStopsDirect = (GradientStop[])gradientStops.Clone();
            GradientStops = GradientStopsDirect;
            Gamma = gamma;
            ExtendMode = extendMode;
        }

        public IReadOnlyList<GradientStop> GradientStops { get; }

        public Gamma Gamma { get; }

        public ExtendMode ExtendMode { get; }

        internal GradientStop[] GradientStopsDirect { get; }

        internal const int MaximumGradientStops = 32;

    }
}
