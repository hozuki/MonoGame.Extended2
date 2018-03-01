using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace MonoGame.Extended.WinForms.Extensions {
    public static class ServiceProviderExtensions {

        [DebuggerStepThrough]
        [CanBeNull]
        public static TService GetService<TService>([NotNull] this IServiceProvider provider) where TService : class {
            return provider.GetService(typeof(TService)) as TService;
        }

    }
}
