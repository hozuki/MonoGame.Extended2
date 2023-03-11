using System;

namespace MonoGame.Extended.WinForms;

public sealed class ServiceEventArgs : EventArgs
{

    public ServiceEventArgs(Type type)
    {
        Guard.ArgumentNotNull(type, nameof(type));

        ServiceType = type;
    }

    public Type ServiceType { get; }

}
