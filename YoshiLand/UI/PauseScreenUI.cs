using Microsoft.Xna.Framework;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace YoshiLand.UI
{
    internal class PauseScreenUI : Panel
    {
        public PauseScreenUI() : base(Anchor.TopLeft, new Vector2(GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height))
        {
            Texture = null;
            AddChild(new Paragraph(Anchor.Center, 1, Language.Strings.Paused, true));
        }
    }
}