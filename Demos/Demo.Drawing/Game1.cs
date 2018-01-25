using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Drawing;
using MonoGame.Extended.Drawing.Geometries;

namespace Demo.Drawing {
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
            IsMouseVisible = true;

            // https://gamedev.stackexchange.com/questions/93447/how-to-correctly-enable-anti-aliasing-in-xna
            //            GraphicsDevice.PresentationParameters.MultiSampleCount = 2;
            _graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            _graphicsDeviceManager.PreferMultiSampling = true;
            _graphicsDeviceManager.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // TODO: use this.Content to load your game content here
            _drawingContext = new DrawingContext(GraphicsDevice);

            _brush1 = new SolidColorBrush(_drawingContext, Color.Black);
            _brush2 = new SolidColorBrush(_drawingContext, Color.Red);
            _brush3 = new SolidColorBrush(_drawingContext, Color.Yellow);

            var pathGeometry = new PathGeometry();
            _pathGeometry1 = pathGeometry;

            var sink = pathGeometry.Open();
            sink.BeginFigure(Vector2.Zero);
            sink.AddLine(new Vector2(100, 100));
            sink.AddLine(new Vector2(0, 100));
            sink.EndFigure(FigureEnd.Closed);
            sink.Close();

            pathGeometry = new PathGeometry();
            _pathGeometry2 = pathGeometry;

            sink = pathGeometry.Open();
            sink.BeginFigure(Vector2.Zero);
            sink.AddBezier(new BezierSegment {
                Point1 = new Vector2(100, 0),
                Point2 = new Vector2(0, 100),
                Point3 = new Vector2(100, 100)
            });
            sink.AddLine(new Vector2(100, 0));
            sink.EndFigure(FigureEnd.Closed);
            sink.Close();

            pathGeometry = new PathGeometry();
            _pathGeometry3 = pathGeometry;

            sink = pathGeometry.Open();
            sink.BeginFigure(new Vector2(200, 200));
            sink.AddArc(new ArcSegment {
                Size = new Vector2(50, 50),
                ArcSize = ArcSize.Large,
                Point = new Vector2(300, 300),
                RotationAngle = 0,
                SweepDirection = SweepDirection.Clockwise
            });
            sink.AddArc(new ArcSegment {
                Size = new Vector2(100, 100),
                ArcSize = ArcSize.Large,
                Point = new Vector2(100, 200),
                RotationAngle = 0,
                SweepDirection = SweepDirection.Clockwise
            });
            sink.EndFigure(FigureEnd.Closed);
            sink.Close();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
            _brush1.Dispose();
            _brush2.Dispose();
            _brush3.Dispose();
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
            // https://stackoverflow.com/questions/33977226/drawing-bezier-curves-in-monogame-xna-produces-scratchy-lines ?
            DrawingContext.FillGeometry(_brush1, _pathGeometry1);
            DrawingContext.FillGeometry(_brush2, _pathGeometry2);
            DrawingContext.FillGeometry(_brush3, _pathGeometry3);

            var fps = 1 / gameTime.ElapsedGameTime.TotalSeconds;
            Window.Title = "FPS: " + fps.ToString("0.00");

            base.Draw(gameTime);
        }

        private readonly GraphicsDeviceManager _graphicsDeviceManager;

        private DrawingContext _drawingContext;
        private Brush _brush1;
        private Brush _brush2;
        private Brush _brush3;

        private PathGeometry _pathGeometry1;
        private PathGeometry _pathGeometry2;
        private PathGeometry _pathGeometry3;

    }
}
