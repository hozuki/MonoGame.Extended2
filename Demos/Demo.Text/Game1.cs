using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Text;
using MonoGame.Extended.Text.Extensions;

namespace Demo.Text {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
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

            _keyboardStateHandler = new KeyboardStateHandler(this);
            _keyboardStateHandler.KeyUp += _keyboardStateHandler_KeyUp;

            Components.Add(_keyboardStateHandler);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            _fontManager = new FontManager();
            _font = _fontManager.LoadFont("Content/Fonts/NotoSansCJKsc-Regular.otf", 20);
            _spriteFont = new DynamicSpriteFont(GraphicsDevice, _font);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
            _spriteFont.Dispose();
            _fontManager.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            _mouseState = Mouse.GetState();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            const string content = "This is line 1\nGreetings everyone\nHashtag # works yay\n还有中文测试";

            const float left1 = 0, left2 = 400;

            spriteBatch.DrawString(_spriteFont, "Mouse location: " + _mouseState.Position, new Vector2(left1, 0), Color.White);
            spriteBatch.DrawString(_spriteFont, "Max bounds: " + _maxBounds, new Vector2(left2, 0), Color.White);

            spriteBatch.DrawString(_spriteFont, "Fixed line height:", new Vector2(left1, 60), Color.Blue);
            var size1 = spriteBatch.DrawString(_spriteFont, content, new Vector2(left1, 100), _maxBounds, 1, 30, Color.White);
            spriteBatch.DrawString(_spriteFont, "Measured size: " + size1, new Vector2(left1, 240), Color.Green);

            spriteBatch.DrawString(_spriteFont, "Minimized line height:", new Vector2(left2, 60), Color.Yellow);
            var size2 = spriteBatch.DrawString(_spriteFont, content, new Vector2(left2, 100), _maxBounds, new Vector2(1, 1), Color.White);
            spriteBatch.DrawString(_spriteFont, "Measured size: " + size2, new Vector2(left2, 240), Color.Green);

            var fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            spriteBatch.DrawString(_spriteFont, "FPS: " + fps.ToString("0.00"), new Vector2(left1, 300), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void _keyboardStateHandler_KeyUp(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
                case Keys.D:
                    _maxBounds.X += 10;
                    break;
                case Keys.A:
                    _maxBounds.X -= 10;
                    break;
                case Keys.W:
                    _maxBounds.Y += 10;
                    break;
                case Keys.S:
                    _maxBounds.Y -= 10;
                    break;
            }
        }

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private MouseState _mouseState;
        private Vector2 _maxBounds = new Vector2(300, 150);

        private KeyboardStateHandler _keyboardStateHandler;

        private FontManager _fontManager;
        private Font _font;
        private DynamicSpriteFont _spriteFont;

    }
}
