using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.UI.Chat;
using Waybound.Common.Utils;

namespace Waybound.Common.TagHandlers;

public class Bar : ITagHandler {
    internal class BarHandler : TextSnippet {
        internal BarHandler(string text) => _anim = text != " FullBar";

        readonly bool _anim = false;

        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1) {
            if (!justCheckingString) {
                Asset<Texture2D>[] asset = Resources.Textures.Extaras;
                int barSize = asset[1].Value.Width;
                if (_anim) {
                    barSize = (int)(Main.GlobalTimeWrappedHourly * 6) % (asset[1].Value.Width * 2);
                    if (barSize >= asset[1].Value.Width - 1) { barSize = asset[1].Value.Width * 2 - barSize; };
                };
                UI.DrawTexture(spriteBatch, asset[2].Value, position, null, origin: Vector2.Zero);
                UI.DrawTexture(spriteBatch, asset[1].Value, position.X(4).Y(4), new(0, 0, barSize, asset[1].Value.Height), origin: Vector2.Zero);
                if (_anim && (barSize > 2 && barSize <= 26)) { UI.DrawTexture(spriteBatch, asset[3].Value, position.X(4 + barSize).Y(4), origin: Vector2.Zero); };
            };
            size = new Vector2(Resources.Textures.Extaras[2].Value.Size().X, Resources.Textures.Extaras[2].Value.Size().Y * 1.5f);
            return true;
        }
        public override float GetStringLength(DynamicSpriteFont font) => (Resources.Textures.Extaras[2].Value.Size() / 2).X * Scale * 1f;
    }
    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options) { return new BarHandler(text) { Text = "" }; }
};