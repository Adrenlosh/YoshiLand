using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using YoshiLand.Systems;
using YoshiLand.UI;

namespace YoshiLand.Screens
{
    public class TitleScreen : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private TitleScreenUI _ui;
        private Texture2D _logo;
        private Texture2D _backgroundPattern;

        public new GameMain Game => (GameMain)base.Game;

        public TitleScreen(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            GameMain.UiSystem.Remove("Root");
            base.Initialize();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        private void InitializeUI()
        {
            _ui = new TitleScreenUI();
            _ui.StartButtonClicked += (s, e) => Game.Screens.ReplaceScreen(new NewMapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            GameMain.UiSystem.Add("Root", _ui);
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _logo = Content.Load<Texture2D>("Images/logo");
            _backgroundPattern = Content.Load<Texture2D>("Images/background-pattern");
            SongSystem.Play("title");
            InitializeUI();
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (GameControllerSystem.BackPressed())
            {
                Game.Exit();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(113, 191, 69));
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Rectangle screenBounds = GraphicsDevice.PresentationParameters.Bounds;
            Rectangle sourceRect = new Rectangle(0, 0, screenBounds.Width, screenBounds.Height);
            _spriteBatch.Draw(_backgroundPattern, screenBounds, sourceRect, Color.White);
            _spriteBatch.End();
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: GameMain.ViewportAdapter.GetScaleMatrix());
            _spriteBatch.Draw(_logo, new Vector2((GameMain.ViewportAdapter.VirtualWidth - _logo.Width) / 2, 50), Color.White);
            _spriteBatch.End();
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }
    }
}