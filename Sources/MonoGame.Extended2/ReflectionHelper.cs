using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace MonoGame.Extended {
    public static class ReflectionHelper {

        public static byte[] LoadResource([NotNull] Assembly assembly, [NotNull] string resourceName) {
            Guard.EnsureArgumentNotNull(assembly, nameof(assembly));
            Guard.EnsureArgumentNotNullOrEmpty(resourceName, nameof(resourceName));

            byte[] data;

            var names = assembly.GetManifestResourceNames();
            using (var resourceStream = assembly.GetManifestResourceStream(resourceName)) {
                Debug.Assert(resourceStream != null, nameof(resourceStream) + " != null");

                using (var memoryStream = new MemoryStream()) {
                    resourceStream.CopyTo(memoryStream);
                    data = memoryStream.ToArray();
                }
            }

            return data;
        }

    }
}
