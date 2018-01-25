using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MonoGame.Extended {
    public static class Guard {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("argValue: null => halt")]
        public static void EnsureArgumentNotNull<T>(T argValue, [NotNull, InvokerParameterName] string paramName)
            where T : class {
            if (paramName == null) {
                throw new ArgumentNullException(nameof(paramName));
            }

            if (ReferenceEquals(argValue, null)) {
                throw new ArgumentNullException(nameof(argValue), "The argument must not be null.");
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("argValue: null => halt")]
        public static void EnsureArgumentNotNullOrEmpty([NotNull] string argValue, [NotNull, InvokerParameterName] string paramName) {
            if (paramName == null) {
                throw new ArgumentNullException(nameof(paramName));
            }

            if (string.IsNullOrEmpty(argValue)) {
                throw new ArgumentException("The argument must not be null or empty.", paramName);
            }
        }

    }
}
