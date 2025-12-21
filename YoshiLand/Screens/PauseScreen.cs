using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System;
using System.Collections.Generic;
using System.Text;
using YoshiLand.Systems;
using YoshiLand.UI;

namespace YoshiLand.Screens
{
    public class PauseScreen : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private PauseScreenUI _ui;

        public new GameMain Game => (GameMain)base.Game;

        public PauseScreen(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            SFXSystem.Play("pause");
            SongSystem.Pause();
            base.Initialize();
        }

        public void InitializeUI()
        {
            _ui = new PauseScreenUI();
            GameMain.UiSystem.Add("PausePanel", _ui);
            GameMain.UiSystem.Get("Root")?.Element.IsHidden = true;
        }
        public override void UnloadContent()
        {
            GameMain.UiSystem.Remove("PausePanel");
            GameMain.UiSystem.Get("Root")?.Element.IsHidden = false;
            base.UnloadContent();
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            InitializeUI();
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(Color.Black, 0.8f));
            //_spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: GameMain.ViewportAdapter.GetScaleMatrix());
            //_spriteBatch.DrawRectangle(Game.GraphicsDevice.Viewport.Bounds, new Color(Color.Black, 0.8f));
            //_spriteBatch.End();
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (GameControllerSystem.StartPressed())
            {
                SFXSystem.Play("pause");
                SongSystem.Resume();
                Game.Screens.CloseScreen(new FadeTransition(GraphicsDevice, Color.Black, 0.3f));
            }
            //_ui.Update(gameTime);
        }
    }
}