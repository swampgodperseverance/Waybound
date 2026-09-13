using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Waybound.Common.Utils;
using Waybound.Content.Race;
using Waybound.Core;

namespace Waybound.UIs.Elements;

public class RaceSlot(PlayerCreationData data) {
    static RaceInfo _clon = null;

    float _scale = 1;

    static int _charCountInfo = 0;
    static int _charCountPozitiv = 0;
    static int _charCountNegativ = 0;

    bool _hover = false;
    bool _tick = false;

    public void DrawSlot(SpriteBatch sb, Asset<Texture2D> raceMicroIcon, RaceInfo race, Vector2 pos, string text, bool big = false) {
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        int sclot = big ? 0 : 2;
        float bigScale = big ? 0.6f : 0;
        UI.DrawTexture(sb, asset[sclot].Value, pos, scale: _scale);
        if (_hover) { UI.DrawTexture(sb, asset[sclot + 1].Value, pos, scale: _scale, color: Color.Gold); };
        UI.DrawTexture(sb, raceMicroIcon.Value, pos, scale: _scale + bigScale);
        if (UI.Hover(pos, asset[sclot].Value)) {
            if (!_tick) {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _tick = true;
            };
            _hover = true;
            _scale = MathHelper.Clamp(_scale + 0.01f, 1f, 1.13f);
            if (_scale < 0.01f) { _scale = 0f; };
            if (UI.LeftClick() && !big) {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                data.race = race;
                data.asset = raceMicroIcon;
            };
            Main.instance.MouseText(text);
        }
        else {
            _scale = MathHelper.Clamp(_scale - 0.01f, 1f, 1.13f);
            if (_scale < 1f) { _scale = 1f; };
            _tick = false;
            _hover = false;
        };
    }
    public static void DrawText(SpriteBatch sB, RaceInfo race, Vector2 pos) {
        string info = race.Info;
        string pozitiv = race.Pozitiv;
        string negativ = race.Negativ;

        if (_clon != race) {
            _charCountInfo = 0;
            _charCountNegativ = 0;
            _charCountPozitiv = 0;
            _clon = race;
        };

        _charCountInfo = Utils.Clamp(_charCountInfo, 0, info.Length);
        _charCountPozitiv = Utils.Clamp(_charCountPozitiv, 0, pozitiv.Length);
        _charCountNegativ = Utils.Clamp(_charCountNegativ, 0, negativ.Length);

        UI.DrawText(sB, info[.._charCountInfo], pos, color: Color.White, scale: new(.75f));
        if (_charCountInfo >= info.Length) {
            UI.DrawText(sB, pozitiv[.._charCountPozitiv], pos.Y(FontAssets.MouseText.Value.MeasureString(info).Y * .76f), color: Color.Green, scale: new(.75f));
            _charCountPozitiv++;
        };
        if (_charCountPozitiv >= pozitiv.Length) {
            UI.DrawText(sB, negativ[.._charCountNegativ], pos.Y(FontAssets.MouseText.Value.MeasureString(info).Y * .76f + FontAssets.MouseText.Value.MeasureString(pozitiv).Y * .76f), color: Color.Red, scale: new(.75f));
            _charCountNegativ++;
        };
        _charCountInfo++;
    }
};