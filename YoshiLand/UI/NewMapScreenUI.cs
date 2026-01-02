using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Formatting;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace YoshiLand.UI
{
    public class NewMapScreenUI : Panel
    {
        private Panel _generalPanel;
        private Paragraph _stageParagraph;
        public NewMapScreenUI(Texture2D map) : base(Anchor.TopLeft, new Vector2(GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height))
        {
            Texture = null;
            _generalPanel = AddChild(new Panel(Anchor.Center, new Vector2(250, 220)));
            _stageParagraph = _generalPanel.AddChild(new Paragraph(Anchor.TopCenter, 246, "Show the name of stage. 你好，世界！", TextAlignment.Center));

            Panel borderPanel = _generalPanel.AddChild(new Panel(Anchor.Center, new Vector2(map.Width + 2, map.Height + 2)));
            borderPanel.AddChild(new Image(Anchor.Center, new Vector2(map.Width, map.Height), new TextureRegion(map)));
        }
    }
}