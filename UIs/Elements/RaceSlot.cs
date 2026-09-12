using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Waybound.Common.Utils;
using Waybound.Content.Race;
using Waybound.Core;

namespace Waybound.UIs.Elements;

public class RaceSlot(PlayerCreationData data) {
    float scale = 1;

    public bool hover = false;
    bool _tick = false;

    public void DrawSlot(SpriteBatch sb, Asset<Texture2D> raceMicroIcon, RaceInfo race, Vector2 pos, string text, bool big = false) {
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        int sclot = big ? 0 : 2;
        UI.DrawTexture(sb, asset[sclot].Value, pos, scale: scale);
        if (hover) { UI.DrawTexture(sb, asset[sclot + 1].Value, pos, scale: scale, color: Color.Gold); };
        UI.DrawTexture(sb, raceMicroIcon.Value, pos, scale: scale);
        if (UI.Hover(pos, asset[sclot].Value)) {
            if (!_tick) {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _tick = true;
            }
            hover = true;
            scale = MathHelper.Clamp(scale + 0.01f, 1f, 1.13f);
            if (scale < 0.01f) { scale = 0f; };
            if (UI.LeftClick()) {
                data.race = race;
            }
            Main.instance.MouseText(text);
        }
        else {
            scale = MathHelper.Clamp(scale - 0.01f, 1f, 1.13f);
            if (scale < 1f) { scale = 1f; };
            _tick = false;
            hover = false;
        }
    }
}