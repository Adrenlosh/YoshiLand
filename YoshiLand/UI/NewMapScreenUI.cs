using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Animations;
using MLEM.Formatting;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using System;
using YoshiLand.Models;

namespace YoshiLand.UI
{
    public class NewMapScreenUI : Panel
    {
        private Panel _generalPanel;
        private Paragraph _stageParagraph;
        private Image _thumbnailImage;
        private Panel _borderPanel;
        private Paragraph _worldParagraph;
        private SpriteAnimationImage _animationImage;

        public event Action<int> OnStageSelected;
        public event Action OnExitPressed;
        public NewMapScreenUI(World world, Texture2D mapYoshiTexture) : base(Anchor.TopLeft, new Vector2(GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height))
        {
            Texture = null;

            TextureRegion[] animation =
            [
                new TextureRegion(mapYoshiTexture, new Rectangle(112, 0, 16, 16)),
                new TextureRegion(mapYoshiTexture, new Rectangle(128, 0, 16, 16)),
                new TextureRegion(mapYoshiTexture, new Rectangle(96, 0, 16, 16)),
            ];
            _animationImage = new SpriteAnimationImage(Anchor.TopLeft, new Vector2(16), new SpriteAnimation(0.2f, animation)) { PositionOffset = world.ThumbnailPositions[0] };

            _worldParagraph = AddChild(new Paragraph(Anchor.TopCenter, GlobalConfig.VirtualResolution_Width, world.DisplayName, TextAlignment.Center){ TextColor = Color.Black }) ;

            _generalPanel = AddChild(new Panel(Anchor.Center, new Vector2(250, 220)));

            _stageParagraph = _generalPanel.AddChild(new Paragraph(Anchor.AutoLeft, 246, Language.Strings.Exit, TextAlignment.Center));

            _generalPanel.AddChild(new VerticalSpace(3));

            _borderPanel = _generalPanel.AddChild(new Panel(Anchor.AutoCenter, new Vector2(world.Thumbnail.Width + 2, world.Thumbnail.Height + 2)));

            _thumbnailImage = _borderPanel.AddChild(new Image(Anchor.Center, new Vector2(world.Thumbnail.Width, world.Thumbnail.Height), new TextureRegion(world.Thumbnail)));
            _borderPanel.AddChild(_animationImage);

            _generalPanel.AddChild(new VerticalSpace(10));

            Panel buttonPanel = _generalPanel.AddChild(new Panel(Anchor.AutoCenter, new Vector2(250, 49)));

            var backButton = buttonPanel.AddChild(new Button(Anchor.AutoLeft, new Vector2(30, 40), Language.Strings.Exit));
            backButton.OnSelected += (b) => { _stageParagraph.Text = Language.Strings.Exit; };
            backButton.OnPressed += (b) => { OnExitPressed?.Invoke(); };

            for (int i = 1; i <= 5; i++)
            {
                var stageButton = buttonPanel.AddChild(new Button(Anchor.AutoInlineIgnoreOverflow, new Vector2(30, 40), i.ToString())
                {
                    PositionOffset = new Vector2(10, 0)
                });

                stageButton.SetData("Index", i - 1);

                stageButton.OnPressed += (b) =>
                {
                    int index = b.GetData<int>("Index");
                    if (index >= 0 && index < world.Stages.Count)
                    {
                        OnStageSelected?.Invoke(index);
                    }
                };

                stageButton.OnSelected += (b) =>
                {
                    int index = b.GetData<int>("Index");
                    if (index >= 0 && index < world.Stages.Count)
                    {
                        _stageParagraph.Text = world.Stages[index].DisplayName;
                        _animationImage.PositionOffset = world.ThumbnailPositions[index];
                    }
                };
            }
        }
    }
}