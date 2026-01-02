using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using YoshiLand.UI;

namespace YoshiLand.Screens
{
    public class NewMapScreen : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _backgroundPattern;
        private NewMapScreenUI _ui;


        public new GameMain Game => (GameMain)base.Game;
        public NewMapScreen(Game game) : base(game)
        {
        }

        public void InitializeUI()
        {
            _ui = new NewMapScreenUI(Content.Load<Texture2D>("Images/map"));
            GameMain.UiSystem.Add("Root", _ui);
        }

        public override void Initialize()
        {
            GameMain.UiSystem.Remove("Root");
            base.Initialize();
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _backgroundPattern = Content.Load<Texture2D>("Images/background-pattern");
            InitializeUI();
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(113, 191, 69));
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Rectangle screenBounds = GraphicsDevice.PresentationParameters.Bounds;
            Rectangle sourceRect = new Rectangle(0, 0, screenBounds.Width, screenBounds.Height);
            _spriteBatch.Draw(_backgroundPattern, screenBounds, sourceRect, Color.White);
            _spriteBatch.End();
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}
