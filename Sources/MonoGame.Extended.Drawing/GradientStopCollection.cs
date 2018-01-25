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

            GradientStops = gradientStops;
            Gamma = gamma;
            ExtendMode = extendMode;
        }

        public IReadOnlyList<GradientStop> GradientStops { get; }

        public Gamma Gamma { get; }

        public ExtendMode ExtendMode { get; }

        private const int MaximumGradientStops = 16;

    }
}
