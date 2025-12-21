using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extended.Font;
using MLEM.Ui;
using MLEM.Ui.Style;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.ViewportAdapters;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Structs;
using System;
using System.Linq;
using YoshiLand.Screens;
using YoshiLand.Status;
using YoshiLand.Systems;

namespace YoshiLand //TODO:屏幕逻辑更新
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;
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
            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = Language.Strings.GameName;
        }

        protected override void Initialize()
        {
            base.Initialize();
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
            InitializeAudio(out MiniAudioEngine engine, out AudioPlaybackDevice playbackDevice);
            StageSystem.Initialize(Content);
            SFXSystem.Initialize(Content, engine, playbackDevice);
            SongSystem.Initialize(Content, engine, playbackDevice);

#if !DEBUG
            _screenManager.ShowScreen(new LogoScreen(this));
#else
            //LoadScreen(new TitleScreen(this));
            //LoadScreen(new GamingScreen(this, StageSystem.GetStageByName("grassland1")));
            _screenManager.ShowScreen(new TitleScreen(this));
#endif
            base.LoadContent();
        }

        private void InitializeAudio(out MiniAudioEngine engine, out AudioPlaybackDevice device)
        {
            engine = new MiniAudioEngine();
            DeviceInfo defaultDevice = engine.PlaybackDevices.FirstOrDefault(x => x.IsDefault);
            device = engine.InitializePlaybackDevice(defaultDevice, AudioFormat.Dvd);
            device.Start();
        }

        protected override void UnloadContent()
        {
            SongSystem.Dispose();
            SFXSystem.Dispose();
            base.UnloadContent();
        }

        //public void LoadScreen(GameScreen screen, Transition transition = null)
        //{
        //    if (transition != null)
        //    {
        //        _screenManager.LoadScreen(screen, transition);
        //    }
        //    else
        //    {
        //        _screenManager.LoadScreen(screen);
        //    }
        //}

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