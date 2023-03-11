using System;
using System.Diagnostics;

namespace MonoGame.Extended.WinForms.Extensions;

public static class ServiceProviderExtensions
{

    [DebuggerStepThrough]
    public static TService? GetService<TService>(this IServiceProvider provider)
        where TService : class
    {
        return provider.GetService(typeof(TService)) as TService;
    }

}
