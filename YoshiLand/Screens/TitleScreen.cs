using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System;
using YoshiLand.Enums;
using YoshiLand.Models;
using YoshiLand.Rendering;
using YoshiLand.Systems;
using YoshiLand.Transitions;
using YoshiLand.UI;

namespace YoshiLand.Screens
{
    public class TitleScreen : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private TitleScreenUI _ui;
        private Stage _stage;
        private MaskTransition _maskTransition;

        private GameSceneRender _gameSceneRenderer;
        private InteractionSystem _interactionSystem;
        private Texture2D _logo;

        private TransitionTimer t = new TransitionTimer(1f);

        public new GameMain Game => (GameMain)base.Game;

        public TitleScreen(Game game) : base(game)
        {
            t.Completed += (object obj, EventArgs e) => { t.Start(); };
        }

        public override void Initialize()
        {
            GameMain.UiSystem.Remove("Root");
            base.Initialize();
        }

        public override void UnloadContent()
        {
            _stage.CloseStage();
            base.UnloadContent();
        }

        private void InitializeUI()
        {
            _ui = new TitleScreenUI();
            _ui.StartButtonClicked += (s, e) => Game.Screens.ReplaceScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            GameMain.UiSystem.Add("Root", _ui);
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _logo = Content.Load<Texture2D>("Images/logo");
            _maskTransition = new MaskTransition(GraphicsDevice, Content, TransitionType.In | TransitionType.Out, 2);
            _stage = StageSystem.GetStageByName("grassland1");
            _gameSceneRenderer = new GameSceneRender(GraphicsDevice, Game.Window, Content);
            _gameSceneRenderer.LoadContent();
            _gameSceneRenderer.LoadMap(_stage.StartStage());
            _interactionSystem = new InteractionSystem();
            GameObjectsSystem.Player.CanHandleInput = true;
            SongSystem.Play("title");
            InitializeUI();
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            GameObjectsSystem.InactivateObejcts(_gameSceneRenderer.GetScreenBounds());
            GameObjectsSystem.ActivateObjects(_gameSceneRenderer.GetScreenBounds());
            GameObjectsSystem.Update(gameTime);
            _maskTransition.Update(gameTime);
            _interactionSystem.Update(gameTime);
            _gameSceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position, true, GameObjectsSystem.Player.FaceDirection, GameObjectsSystem.Player.Velocity);
            t.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _gameSceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects());
            _maskTransition.Draw(gameTime);
            _spriteBatch.Begin(transformMatrix: _gameSceneRenderer.ViewportAdapter.GetScaleMatrix());
            if (_maskTransition.State == TransitionState.Out && _maskTransition.Value != 0)
            {
                _spriteBatch.FillRectangle(_gameSceneRenderer.ViewportAdapter.BoundingRectangle, Color.Black);
            }
            _spriteBatch.End();
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: GameMain.ViewportAdapter.GetScaleMatrix());
            _spriteBatch.Draw(_logo, new Vector2((GameMain.ViewportAdapter.VirtualWidth - _logo.Width) / 2, 50), Rainbow(t.Value));
            _spriteBatch.End();
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }

        public static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return new Color(255, 255, ascending, 0);
                case 1:
                    return new Color(255, descending, 255, 0);
                case 2:
                    return new Color(255, 0, 255, ascending);
                case 3:
                    return new Color(255, 0, descending, 255);
                case 4:
                    return new Color(255, ascending, 0, 255);
                default: // case 5:
                    return new Color(255, 255, 0, descending);
            }
        }
    }
}