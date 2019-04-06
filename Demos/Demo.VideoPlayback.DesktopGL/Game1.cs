//#define TEST_ASS
//#define TEST_SRT

using System;
using Demo.VideoPlayback.AssSubtitle;
using Demo.VideoPlayback.SrtSubtitle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Framework.Media;
using MonoGame.Extended.VideoPlayback;

namespace Demo.VideoPlayback.DesktopGL {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public sealed class Game1 : Game {

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // TODO: Add your initialization logic here
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = WindowWidth;
            _graphics.PreferredBackBufferHeight = WindowHeight;
            _graphics.ApplyChanges();

            _videoPlayer = new VideoPlayer(GraphicsDevice);
            //_videoPlayer.IsLooped = true;

            _keyboardStateHandler = new KeyboardStateHandler(this);

            _keyboardStateHandler.KeyUp += KeyboardStateHandler_KeyUp;

            Components.Add(_keyboardStateHandler);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            _helpTexture = TextureLoader.LoadTexture(GraphicsDevice, "Content/HelpTexture.png");

#if TEST_ASS
            _video = VideoHelper.LoadFromFile(@"D:\Videos\sm32324383.mp4");

            var subtitle = new AssSubtitleRenderer();

            subtitle.LoadFromFile(@"D:\Videos\sm32324383.ass");
            subtitle.Enabled = true;

            _videoPlayer.SubtitleRenderer = subtitle;
#elif TEST_SRT
            _video = VideoHelper.LoadFromFile(@"D:\TEMP\test_mp4.mp4");

            var subtitle = new SrtSubtitleRenderer(GraphicsDevice);

            subtitle.LoadFromFile(@"D:\TEMP\test_mp4.srt");
            subtitle.ApplyFontFile(@"C:\Windows\Fonts\arial.ttf");
            subtitle.Enabled = true;

            _videoPlayer.SubtitleRenderer = subtitle;
#else
            _video = VideoHelper.LoadFromFile("Content/SampleVideo_1280x720_1mb.mp4");
#endif

            _videoPlayer.Play(_video);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
            _helpTexture.Dispose();
            _videoPlayer.Stop();
            _video.Dispose();
            _videoPlayer.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            var gamePadState = GamePad.GetState(PlayerIndex.One);

            if (gamePadState.Buttons.Back == ButtonState.Pressed) {
                Exit();
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            var videoTexture = _videoPlayer.GetTexture();

            _spriteBatch.Begin();

            var destRect = new Rectangle(0, 0, WindowWidth, WindowHeight);
            _spriteBatch.Draw(videoTexture, destRect, Color.White);

            _spriteBatch.End();

            _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
            _spriteBatch.Draw(_helpTexture, Vector2.Zero, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing) {
            _video.Dispose();
            _videoPlayer.Dispose();

            _keyboardStateHandler.KeyUp -= KeyboardStateHandler_KeyUp;

            base.Dispose(disposing);
        }

        private void KeyboardStateHandler_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Escape) {
                Exit();
            } else if (e.KeyCode == Keys.Space) {
                var videoState = _videoPlayer.State;

                switch (videoState) {
                    case MediaState.Stopped:
                        _videoPlayer.Play(_video);
                        break;
                    case MediaState.Playing:
                        _videoPlayer.Pause();
                        break;
                    case MediaState.Paused:
                        _videoPlayer.Resume();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } else if (e.KeyCode == Keys.R) {
                _videoPlayer.Replay();
            }
        }

        private const int WindowWidth = 1024;
        private const int WindowHeight = 576;

        private KeyboardStateHandler _keyboardStateHandler;

        private Video _video;
        private VideoPlayer _videoPlayer;

        private Texture2D _helpTexture;

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

    }
}
