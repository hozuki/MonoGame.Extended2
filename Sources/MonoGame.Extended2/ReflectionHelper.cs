using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace MonoGame.Extended {
    public static class ReflectionHelper {

        [NotNull]
        public static byte[] LoadResource([NotNull] Assembly assembly, [NotNull] string resourceName) {
            Guard.ArgumentNotNull(assembly, nameof(assembly));
            Guard.NotNullOrEmpty(resourceName, nameof(resourceName));

            byte[] data;

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
