using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Overlay;

namespace Demo.Overlay {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {

        public Game1() {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
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
            _graphics = new Graphics(GraphicsDevice);
            _fontManager = new FontManager();

            var font = _fontManager.LoadFont("Content/Fonts/NotoSansCJKsc-Regular.otf");
            font.FakeBold = true;
            font.Size = 50;

            var whiteBrush = new SolidBrush(Color.White);
            var translucentGreenBrush = new SolidBrush(Color.FromNonPremultiplied(0, 255, 0, 127));
            var redPen = new Pen(Color.Red, 2);
            var blackPen = new Pen(Color.Black, 10);

            const string testStr = "Hello, world! 你好，世界！";
            var textPos = new Vector2(200, 200);
            _graphics.DrawString(blackPen, font, testStr, textPos);
            _graphics.FillString(whiteBrush, font, testStr, textPos);

            var rect = new Rectangle(50, 50, 100, 100);

            _graphics.FillRectangle(translucentGreenBrush, rect);
            _graphics.DrawRectangle(redPen, rect);

            var lgp1 = new Vector2(0, 0);
            var lgp2 = new Vector2(50, 50);
            var lgb1 = new LinearGradientBrush(lgp1, lgp2, Color.Red, Color.Blue);

            _graphics.FillRectangle(lgb1, 0, 0, 60, 60);

            lgb1.LinearColors = new[] { lgb1.LinearColors[0], Color.Green };

            _graphics.FillRectangle(lgb1, 0, 50, 30, 30);

            IsMouseVisible = true;

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
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
            _graphics.Dispose();
            _fontManager.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape)) {
                Exit();
            }

            // TODO: Add your update logic here
            _graphics?.UpdateBackBuffer();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.Draw(_graphics.BackBuffer, Vector2.Zero, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;

        private FontManager _fontManager;
        private Graphics _graphics;

    }
}
