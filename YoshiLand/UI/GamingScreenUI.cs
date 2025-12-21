using Microsoft.Xna.Framework;
using MLEM.Ui.Elements;
using MonoGame.Extended.Screens.Transitions;
using System;
using YoshiLand.Systems;
using YoshiLand.Transitions;
using YoshiLand.UI.CustomControls;

namespace YoshiLand.UI
{

    public class GamingScreenUI : Panel
    {
        private TimeSpan? _remainingTime = TimeSpan.FromSeconds(1);
        private MessageBox _messageBox;

        private TransitionTimer _timer = new TransitionTimer(0.6f);

        private Paragraph _LifeLeftParagraph;
        private Paragraph _EggParagraph;
        private Paragraph _CoinParagraph;
        private Paragraph _ScoreParagraph;
        private Paragraph _TimeParagraph;
        private Paragraph _HealthParagraph;
        private Panel _pausePanel;

        public bool IsReadingMessage { get; set; }

        public bool IsPaused { get; set; } = false;

        public bool HandlePause { get; set; } = true;

        public event Action OnMessageBoxClosed;
        public event Action OnCancelPause;

        public GamingScreenUI() : base(MLEM.Ui.Anchor.TopLeft, new Vector2(GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height))
        {
            Texture = null;
            int width = (int)(Size.X / 9 * 2);
            Group paragraphs = new Group(MLEM.Ui.Anchor.TopCenter, Vector2.One) { SetHeightBasedOnChildren = true };
            _LifeLeftParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { TextColor = Color.Orange, GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.LifeLeftOnHud, GameMain.PlayerStatus.LifeLeft); }) };
            _EggParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.EggCountOnHud, GameMain.PlayerStatus.Egg); }) };
            _CoinParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.CoinOnHud, GameMain.PlayerStatus.Coin); }) };
            _ScoreParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.ScoreOnHud, GameMain.PlayerStatus.Score); }) };
            _TimeParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.TimeOnHud, ((int)_remainingTime.Value.TotalSeconds).ToString().PadLeft(3, '0')); }) };
            _HealthParagraph = new Paragraph(MLEM.Ui.Anchor.BottomLeft, 1, string.Empty, true) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.HealthOnHud, GameObjectsSystem.Player.Health, GameObjectsSystem.Player.MaxHealth); }) };
            paragraphs.AddChild(_LifeLeftParagraph);
            paragraphs.AddChild(_EggParagraph);
            paragraphs.AddChild(_CoinParagraph);
            paragraphs.AddChild(_ScoreParagraph);
            paragraphs.AddChild(_TimeParagraph);
            
            _pausePanel = new Panel(MLEM.Ui.Anchor.Center, Size){ IsHidden = true, DrawColor = new Color(Color.Black, 0.7f) };
            _pausePanel.AddChild(new Paragraph(MLEM.Ui.Anchor.Center, 1, Language.Strings.Paused, true));

            _messageBox = new MessageBox();
            _messageBox.OnClosed += () =>
            {
                IsReadingMessage = false;
                OnMessageBoxClosed?.Invoke();
            };

            AddChild(_pausePanel);
            AddChild(paragraphs);
            AddChild(_HealthParagraph);
            AddChild(_messageBox);

            _timer.StateChanged += Timer_StateChanged;
            _timer.Completed += Timer_Completed;
        }

        private void Timer_Completed(object sender, EventArgs e)
        {
            _pausePanel.IsHidden = true;
            IsPaused = false;
        }

        private void Timer_StateChanged(object sender, EventArgs e)
        {
            _timer.Pause();
        }

        public void ShowMessageBox(string messageID)
        {
            IsReadingMessage = true;
            _messageBox.Show(Language.Messages.ResourceManager.GetString(messageID));
        }

        public void Pause()
        {
            SongSystem.Pause();
            SFXSystem.Play("pause");
            IsPaused = true;
            _timer.Start();
            _pausePanel.IsHidden = false;
        }

        public void Unpause()
        {
            SongSystem.Resume();
            SFXSystem.Play("pause");
            IsPaused = false;
            _timer.Resume();
        }

        private void HandleInput()
        {
            //if (GameControllerSystem.StartPressed() && HandlePause && !IsReadingMessage)
            //{
            //    if (IsPaused)
            //    {
            //        Unpause();
            //    }
            //    else
            //    {
            //        Pause();
            //    }
            //}
        }

        public void Update(GameTime gameTime, TimeSpan? remainingTime = null)
        {
            HandleInput();
            _remainingTime = remainingTime;
            _HealthParagraph.TextColor = GameObjectsSystem.Player.Health < 2 ? Color.Red : Color.Yellow;
            _TimeParagraph.TextColor = _remainingTime.Value.TotalSeconds <= 100 ? Color.Red : Color.Yellow;
            if(IsReadingMessage)
            {
                _messageBox.Update();
            }
            _timer.Update(gameTime);
            if(_timer.State == TransitionState.Out || _timer.State == TransitionState.In)
            {
                _pausePanel.DrawColor = new Color(Color.Black, _timer.Value);
            }

        }
    }
}