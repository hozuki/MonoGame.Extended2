using System.Reflection;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.DesktopGL.VideoPlayback {
    /// <summary>
    /// Helper class for <see cref="Game"/>. Internal use only.
    /// </summary>
    internal static class GameHelper {

        /// <summary>
        /// Gets the running <see cref="Game"/> instance. It depends on the property name and lifecycle of <see cref="Game"/>.
        /// </summary>
        /// <returns>The game instance.</returns>
        internal static Game GetCurrentGame() {
            if (_gameInstanceProperty == null) {
                var t = typeof(Game);
                var instanceProp = t.GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic);

                if (instanceProp == null) {
                    return null;
                }

                _gameInstanceProperty = instanceProp;
            }

            var prop = _gameInstanceProperty;
            var instance = (Game)prop.GetValue(null);

            return instance;
        }

        private static PropertyInfo _gameInstanceProperty;

    }
}
