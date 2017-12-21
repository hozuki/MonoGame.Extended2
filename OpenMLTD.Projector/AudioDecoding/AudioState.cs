namespace OpenMLTD.Projector.AudioDecoding {
    /// <summary>
    /// Audio playback state.
    /// </summary>
    internal enum AudioState {

        /// <summary>
        /// The state is unknown.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Audio buffer is loaded.
        /// </summary>
        Loaded = 1,
        /// <summary>
        /// The source is playing.
        /// </summary>
        Playing = 2,
        /// <summary>
        /// The source is paused.
        /// </summary>
        Paused = 3,
        /// <summary>
        /// The source consumed all buffers and stopped.
        /// </summary>
        Stopped = 4

    }
}
