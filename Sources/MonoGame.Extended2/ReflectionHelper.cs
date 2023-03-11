using System.IO;
using System.Reflection;

namespace MonoGame.Extended;

public static class ReflectionHelper
{

    public static byte[]? LoadResource(Assembly assembly, string resourceName)
    {
        Guard.ArgumentNotNull(assembly, nameof(assembly));
        Guard.NotNullOrEmpty(resourceName, nameof(resourceName));

        using var resourceStream = assembly.GetManifestResourceStream(resourceName);

        if (resourceStream is null)
        {
            return null;
        }

        using var memoryStream = new MemoryStream();
        resourceStream.CopyTo(memoryStream);

        var data = memoryStream.ToArray();

        return data;
    }

}
