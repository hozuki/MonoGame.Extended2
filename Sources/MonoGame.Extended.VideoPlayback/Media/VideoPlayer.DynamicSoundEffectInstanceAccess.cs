using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Audio;

// ReSharper disable once CheckNamespace
namespace MonoGame.Extended.Framework.Media {
    partial class VideoPlayer {

        internal struct DynamicSoundEffectInstanceAccess : IDisposable {

            internal DynamicSoundEffectInstanceAccess([NotNull] VideoPlayer videoPlayer) {
                _videoPlayer = videoPlayer;

                Monitor.Enter(videoPlayer._soundEffectInstanceLock);
            }

            [CanBeNull]
            public DynamicSoundEffectInstance SoundEffect {
                [DebuggerStepThrough]
                get => _videoPlayer._soundEffectInstance;
            }

            public void Dispose() {
                Monitor.Exit(_videoPlayer._soundEffectInstanceLock);
            }

            [NotNull]
            private readonly VideoPlayer _videoPlayer;

        }

    }
}
