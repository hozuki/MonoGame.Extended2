using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MonoGame.Extended.WinForms;

public class ServiceContainer : IServiceProvider
{

    public ServiceContainer()
    {
        _lock = new object();
        _registry = new Dictionary<Type, object>();
    }

    public static ServiceContainer Default { get; } = new ServiceContainer();

    public void AddService(Type serviceType, object service)
    {
        Guard.ArgumentNotNull(serviceType, nameof(serviceType));
        Guard.ArgumentNotNull(service, nameof(service));

        lock (_lock)
        {
            _registry[serviceType] = service;
        }

        OnServiceSet(new ServiceEventArgs(serviceType));
    }

    public void AddService<TService>([DisallowNull] TService service)
    {
        Guard.ArgumentNotNull(service, nameof(service));

        AddService(typeof(TService), service);
    }

    public object? GetService(Type serviceType)
    {
        Guard.NotNull(serviceType, nameof(serviceType));

        object? service;

        lock (_lock)
        {
            _registry.TryGetValue(serviceType, out service);
        }

        return service;
    }

    [DebuggerStepThrough]
    public TService? GetService<TService>()
        where TService : class
    {
        return GetService(typeof(TService)) as TService;
    }

    public bool TryGetService(Type serviceType, out object? service)
    {
        Guard.NotNull(serviceType, nameof(serviceType));

        bool got;

        lock (_lock)
        {
            got = _registry.TryGetValue(serviceType, out service);
        }

        return got;
    }

    public bool TryGetService<TService>(out TService? service) where TService : class
    {
        var got = TryGetService(typeof(TService), out var obj);

        service = obj as TService;

        return got;
    }

    public bool RemoveService(Type serviceType)
    {
        Guard.ArgumentNotNull(serviceType, nameof(serviceType));

        bool removed;

        lock (_lock)
        {
            removed = _registry.Remove(serviceType);
        }

        OnServiceSet(new ServiceEventArgs(serviceType));

        return removed;
    }

    public bool RemoveService<TService>()
    {
        bool removed;

        lock (_lock)
        {
            removed = _registry.Remove(typeof(TService));
        }

        OnServiceSet(new ServiceEventArgs(typeof(TService)));

        return removed;
    }

    public event EventHandler<ServiceEventArgs>? RegistryUpdated;

    protected virtual void OnServiceSet(ServiceEventArgs e)
    {
        RegistryUpdated?.Invoke(this, e);
    }

    private readonly object _lock;
    private readonly Dictionary<Type, object> _registry;

}
