using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace MonoGame.Extended.Drawing.Effects;

internal sealed partial class EffectResource
{

    private EffectResource(string effectResourceName)
    {
        ResourceName = effectResourceName;
    }

    public static EffectResource CreateSolidColorBrushEffectResource(DrawingContext drawingContext)
    {
        return CreateEffectResource(drawingContext.Backend, SolidColorBrushResourceNames);
    }

    public static EffectResource CreateLinearGradientBrushEffectResource(DrawingContext drawingContext)
    {
        return CreateEffectResource(drawingContext.Backend, LinearGradientBrushResourceNames);
    }

    public string ResourceName { get; }

    public byte[] Bytecode => _bytecode ??= Load(ResourceName);

    private static EffectResource CreateEffectResource(GraphicsBackend backend, IReadOnlyDictionary<GraphicsBackend, string> resourceMap)
    {
        var resourceName = resourceMap[backend];
        return new EffectResource(resourceName);
    }

    private static byte[] Load(string effectResourceName)
    {
        Guard.NotNullOrEmpty(effectResourceName, nameof(effectResourceName));

        var assembly = Assembly.GetAssembly(typeof(EffectResource))!;

        var data = ReflectionHelper.LoadResource(assembly, effectResourceName);

        if (data is null)
        {
            throw new MissingManifestResourceException($"Resource \"{effectResourceName}\" is missing from the assembly. Please make sure its compile action is set to \"Resource\" so that it can be embedded into the assembly.");
        }

        return data;
    }

    private byte[]? _bytecode;

}
