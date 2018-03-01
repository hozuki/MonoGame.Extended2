using System;
using JetBrains.Annotations;

namespace MonoGame.Extended.WinForms {
    public sealed class ServiceEventArgs : EventArgs {

        public ServiceEventArgs([NotNull] Type type) {
            Guard.ArgumentNotNull(type, nameof(type));

            ServiceType = type;
        }

        [NotNull]
        public Type ServiceType { get; }

    }
}
