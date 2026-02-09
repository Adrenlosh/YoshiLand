using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extended.Font;
using MLEM.Ui;
using MLEM.Ui.Style;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using MonoStereo;
using System;
using YoshiLand.Screens;
using YoshiLand.Status;
using YoshiLand.Systems;

namespace YoshiLand //TODO: **************
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
        private bool _isRunning = true;
        private readonly ScreenManager _screenManager;

        public ScreenManager Screens => _screenManager;

        public static UiSystem UiSystem { get; set; }

        public static ViewportAdapter ViewportAdapter { get; private set; }

        public static PlayerStatus PlayerStatus { get; set; } = new PlayerStatus();

        public GameMain()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = GlobalConfig.VirtualResolution_Width,
                PreferredBackBufferHeight = GlobalConfig.VirtualResolution_Height,
            };
            _graphicsDeviceManager.ApplyChanges();
            MonoStereoEngine.Initialize(() => !_isRunning);
            Services.AddService(_graphicsDeviceManager);
            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = Language.Strings.GameName;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            _isRunning = false;
            base.OnExiting(sender, args);
        }

        private void InitializeUi()
        {
            var uiStyle = new UntexturedStyle(_spriteBatch)
            {
                Font = new GenericBitmapFont(Content.Load<BitmapFont>("Fonts/ZFull-GB")),
            };
            UiSystem = new UiSystem(this, uiStyle, null, false);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }

        protected override void Update(GameTime gameTime)
        {
            UiSystem.GlobalScale = GetUIScale(ViewportAdapter);
            GameControllerSystem.Update();
            VibrationSystem.Update(gameTime);
            SFXSystem.Update(gameTime);
            UiSystem.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void LoadContent()
        {
            ViewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            InitializeUi();
            StagesSystem.Initialize(Content);
            SFXSystem.Initialize(Content);
            SongSystem.Initialize(Content);


#if !DEBUG
            _screenManager.ShowScreen(new LogoScreen(this), new FadeTransition(GraphicsDevice, Color.Black, 1f));
#else
            _screenManager.ShowScreen(new GamingScreen(this, StagesSystem.Worlds[0].Stages[0]));
#endif
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            SongSystem.Dispose();
            SFXSystem.Dispose();
            base.UnloadContent();
        }

        public float GetUIScale(ViewportAdapter viewportAdapter)
        {
            var virtualWidth = GlobalConfig.VirtualResolution_Width;
            var virtualHeight = GlobalConfig.VirtualResolution_Height;
            var scaleX = (float)viewportAdapter.Viewport.Width / virtualWidth;
            var scaleY = (float)viewportAdapter.Viewport.Height / virtualHeight;
            return Math.Min(scaleX, scaleY);
        }
    }
}