using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using YoshiLand.Enums;
using YoshiLand.Systems;
using YoshiLand.Transitions;
using YoshiLand.UI;

namespace YoshiLand.Screens
{
    public class NewMapScreen : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private NewMapScreenUI _ui;

        public new GameMain Game => (GameMain)base.Game;
        public NewMapScreen(Game game) : base(game)
        {
        }

        public void InitializeUI()
        {
            _ui = new NewMapScreenUI(StagesSystem.Worlds[0], Content.Load<Texture2D>("Atlas/map-yoshi"));
            _ui.OnStageSelected += OnStageSelected;
            _ui.OnExitPressed += OnExitPressed;
            GameMain.UiSystem.Add("Root", _ui);
        }

        private void OnExitPressed()
        {
            Game.Screens.ReplaceScreen(new TitleScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
        }

        private void OnStageSelected(int obj)
        {
            SFXSystem.Play("yoshi");
            Game.Screens.ReplaceScreen(new GamingScreen(Game, StagesSystem.Worlds[0].Stages[obj]), new FadeTransition(GraphicsDevice, Color.Black, 1.8f));
        }

        public override void Initialize()
        {
            GameMain.UiSystem.Remove("Root");
            SongSystem.Play("song2");
            base.Initialize();
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            InitializeUI();
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(206, 255, 206));
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (GameControllerSystem.BackPressed())
            {
                Game.Screens.ReplaceScreen(new TitleScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
        }
    }
}
