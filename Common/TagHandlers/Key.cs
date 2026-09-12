using ReLogic.Graphics;
using Terraria;
using Terraria.UI.Chat;
using static Waybound.Common.TagHandlers.Bar;

namespace Waybound.Common.TagHandlers;

public class Key : ITagHandler {
    internal class KeyHandler : TextSnippet {
        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
            if (!justCheckingString && (color.R != 0 || color.G != 0 || color.B != 0)) {

            }
            size = new Vector2(Resources.Textures.Extaras[1].Value.Size().X, Resources.Textures.Extaras[1].Value.Size().Y * 1.5f);
            return true;
        }
        public override float GetStringLength(DynamicSpriteFont font) => (Resources.Textures.Extaras[1].Value.Size() / 2).X * Scale * 1f;
    }
    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options) { return new BarHandler(text) { Text = "" }; }
}
