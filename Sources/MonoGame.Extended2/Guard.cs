using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace MonoGame.Extended {
    public static class Guard {

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("value:null => halt")]
        public static void NotNull<T>([CanBeNull] T value, [NotNull, InvokerParameterName] string paramName) {
            if (paramName == null) {
                throw new ArgumentNullException(nameof(paramName));
            }

            if (ReferenceEquals(value, null)) {
                throw new NullReferenceException($"\"{paramName}\" should not be null.");
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("value:null => halt")]
        public static void NotNullOrEmpty([CanBeNull] string value, [NotNull, InvokerParameterName] string name) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException($"\"{name}\" should not be null or empty.");
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("value:null => halt")]
        public static void ArgumentNotNull<T>([CanBeNull] T value, [NotNull, InvokerParameterName] string paramName) {
            if (paramName == null) {
                throw new ArgumentNullException(nameof(paramName));
            }

            if (ReferenceEquals(value, null)) {
                throw new ArgumentNullException(paramName, $"Argument \"{paramName}\" should not be null.");
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FileExists([CanBeNull] string path) {
            if (path == null) {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path)) {
                throw new FileNotFoundException("Requested file is not found.", path);
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GreaterThan(int value, int limit, [NotNull, InvokerParameterName] string paramName) {
            if (paramName == null) {
                throw new ArgumentNullException(nameof(paramName));
            }

            if (value < limit) {
                throw new ArgumentOutOfRangeException(paramName, $"Value of argument \"{paramName}\" {value} should be greater than limit {limit}.");
            }
        }

    }
}
