using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Drawing;
using MonoGame.Extended.Drawing.Geometries;
using MonoGame.Extended.Text;

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

            _fontManager = new FontManager();
            _font = _fontManager.LoadFont("Content/OpenSans-Regular.ttf", 50);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // TODO: use this.Content to load your game content here
            _drawingContext = new DrawingContext(GraphicsDevice, GraphicsBackend.OpenGL);

            _brush1 = new SolidColorBrush(_drawingContext, Color.Black);
            _brush2 = new SolidColorBrush(_drawingContext, Color.Red);
            _brush3 = new SolidColorBrush(_drawingContext, Color.Yellow);
            _brush4 = new SolidColorBrush(_drawingContext, Color.Purple);
            _brush5 = new SolidColorBrush(_drawingContext, Color.Blue);

            _brush6 = new LinearGradientBrush(_drawingContext,
                new LinearGradientBrushProperties {
                    StartPoint = new Vector2(305, 305),
                    EndPoint = new Vector2(405, 405)
                }, new GradientStopCollection(new[] {
                    new GradientStop { Color = Color.Red, Position = 0.0f },
                    new GradientStop { Color = Color.Yellow, Position = 0.4f },
                    new GradientStop { Color = Color.Blue, Position = 0.8f },
                    new GradientStop { Color = Color.Cyan, Position = 1.0f }
                }, Gamma.Linear, ExtendMode.Mirror));

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

            _ellipseGeometry4 = new EllipseGeometry(new Ellipse {
                Point = new Vector2(400, 50),
                RadiusX = 50,
                RadiusY = 10
            });

            _roundedRectangleGeometry5 = new RoundedRectangleGeometry(new RoundedRectangle {
                RadiusX = 30,
                RadiusY = 20,
                Rectangle = new RectangleF(0, 300, 100, 100)
            });

            _ellipseGeometry6 = new EllipseGeometry(new Ellipse {
                Point = new Vector2(350, 300),
                RadiusX = 160,
                RadiusY = 120
            });

            _fontPathGeometry7 = PathGeometry.CreateFromString(_font, "Press space to start");
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
            _brush4.Dispose();
            _brush5.Dispose();
            _brush6.Dispose();
            _fontManager?.Dispose();
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
            _drawingContext.FillGeometry(_brush1, _pathGeometry1);
            _drawingContext.FillGeometry(_brush2, _pathGeometry2);
            _drawingContext.FillGeometry(_brush3, _pathGeometry3);
            _drawingContext.FillGeometry(_brush4, _ellipseGeometry4);
            _drawingContext.FillGeometry(_brush5, _roundedRectangleGeometry5);
            _drawingContext.FillGeometry(_brush6, _ellipseGeometry6);

            _drawingContext.PushTransform();
            _drawingContext.Translate(0, _font.Size);
            _drawingContext.FillGeometry(_brush6, _fontPathGeometry7);
            _drawingContext.PopTransform();

            var fps = 1 / gameTime.ElapsedGameTime.TotalSeconds;
            Window.Title = "FPS: " + fps.ToString("0.00");

            base.Draw(gameTime);
        }

        private readonly GraphicsDeviceManager _graphicsDeviceManager;

        private DrawingContext _drawingContext;
        private Brush _brush1;
        private Brush _brush2;
        private Brush _brush3;
        private Brush _brush4;
        private Brush _brush5;
        private Brush _brush6;

        private PathGeometry _pathGeometry1;
        private PathGeometry _pathGeometry2;
        private PathGeometry _pathGeometry3;
        private EllipseGeometry _ellipseGeometry4;
        private RoundedRectangleGeometry _roundedRectangleGeometry5;
        private EllipseGeometry _ellipseGeometry6;
        private PathGeometry _fontPathGeometry7;

        private FontManager _fontManager;
        private Font _font;

    }
}
