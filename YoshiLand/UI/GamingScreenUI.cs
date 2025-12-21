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

        private Paragraph _LifeLeftParagraph;
        private Paragraph _EggParagraph;
        private Paragraph _CoinParagraph;
        private Paragraph _ScoreParagraph;
        private Paragraph _TimeParagraph;
        private Paragraph _HealthParagraph;

        public bool IsReadingMessage { get; set; }

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

            _messageBox = new MessageBox();
            _messageBox.OnClosed += () =>
            {
                IsReadingMessage = false;
                OnMessageBoxClosed?.Invoke();
            };

            AddChild(paragraphs);
            AddChild(_HealthParagraph);
            AddChild(_messageBox);
        }

        public void ShowMessageBox(string messageID)
        {
            IsReadingMessage = true;
            _messageBox.Show(Language.Messages.ResourceManager.GetString(messageID));
        }

        public void Update(GameTime gameTime, TimeSpan? remainingTime = null)
        {
            _remainingTime = remainingTime;
            _HealthParagraph.TextColor = GameObjectsSystem.Player.Health < 2 ? Color.Red : Color.Yellow;
            _TimeParagraph.TextColor = _remainingTime.Value.TotalSeconds <= 100 ? Color.Red : Color.Yellow;
            if(IsReadingMessage)
            {
                _messageBox.Update();
            }
        }
    }
}