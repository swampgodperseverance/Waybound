using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Waybound.Common.WUtils;
using Waybound.Content.Race;
using Waybound.Core;
using Waybound.UIs.Elements;

namespace Waybound.UIs;

public class Race(PlayerCreationData data, string text) : UIElement {
    readonly RaceSlot[] raceElement = new RaceSlot[7];

    public int posScaleX = 0;
    public int posScaleY = 0;

    public float alpha = 0f;

    readonly bool[] _tick = new bool[3];

    public override void OnInitialize() {
        for (int i = 0; i < 7; i++) { raceElement[i] = new(data); }
        for (int i = 0; i < 3; i++) { _tick[i] = false; }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch) {
        alpha = MathHelper.Clamp(alpha + 0.02f, 0f, 1f);
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        RaceInfo info = data.race ?? new Human();
        Asset<Texture2D> bigSlotTexture = data.asset ?? asset[11];
        Stat stat = info.GetStat;
        CalculatedStyle dimensions = GetDimensions();
        Vector2 pos = new(dimensions.X + 40 + posScaleX, dimensions.Y + 120 + posScaleY);
        Vector2 textPos = pos.X(94).Y(46);
        Vector2 statPos = pos.X(15).Y(117);
        string raceName = data.race == null ? "" : " (" + info.RaceName + ")";
        int scaleX = 0;

        UI.DrawTexture(spriteBatch, asset[0].Value, pos.X(209).Y(40), color: Color.White * alpha);
        UI.DrawText(spriteBatch, text + raceName, pos.X(250).Y(-43), FontAssets.DeathText.Value, orgin: FontAssets.DeathText.Value.MeasureString(text + raceName) / 2, scale: new(0.55f), color: Color.White * alpha, color1: Color.Black * alpha);

        raceElement[0].DrawSlot(spriteBatch, bigSlotTexture, info, pos.X(20).Y(16), alpha, info.RaceName, true);
        for (int i = 1; i < 6; i++) {
            RaceInfo race = CustomClassData.RaceInfo[i - 1];
            raceElement[i].DrawSlot(spriteBatch, asset[10 + i], race, pos.X(130 + scaleX).Y(6), alpha, race.RaceName);
            scaleX += 70;
        };
        raceElement[6].DrawText(spriteBatch, info, textPos, alpha);

        UI.DrawTexture(spriteBatch, asset[5].Value, statPos, color: Color.White * alpha);
        UI.DrawText(spriteBatch, (100 + stat.BonusHP).ToString(), statPos.X(-4).Y(-30), color: Color.White * alpha);
        UI.DrawText(spriteBatch, (20 + stat.BonusMP).ToString(), statPos.X(-4).Y(8), color: Color.White * alpha);

        UI.DrawTexture(spriteBatch, asset[10].Value, statPos.X(32).Y(2), sourceRectangle: asset[10].Value.Frame(1, 2, 0, 1), color: Color.White * alpha);
        if (UI.Hover(statPos.X(32).Y(2), asset[10].Value.Frame(1, 2, 0, 1))) {
            UI.DrawTexture(spriteBatch, asset[10].Value, statPos.X(32).Y(2), asset[10].Value.Frame(1, 2, 0, 0), color: Color.White * alpha);
            UI.DrawMouseText(spriteBatch, info.ShortNegativ, Color.DarkViolet * alpha);
            if (!_tick[2]) {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _tick[2] = true;
            };
        } else { _tick[2] = false; };
        DrawStat(spriteBatch, statPos.X(-30).Y(-20), Language.GetText("BestiaryInfo.Life").Value, Color.IndianRed * alpha, 6, 0);
        DrawStat(spriteBatch, statPos.X(-30).Y(15), Language.GetText("LegacyInterface.2").Value, Color.CornflowerBlue * alpha, 8, 1);
    }
    void DrawStat(SpriteBatch sB, Vector2 pos, string text, Color color, int indexTexture, int indexBool) {
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        UI.DrawTexture(sB, asset[indexTexture].Value, pos, color: Color.White * alpha);
        if (UI.Hover(pos, asset[indexTexture].Value)) {
            UI.DrawTexture(sB, asset[indexTexture + 1].Value, pos, color: Color.White * alpha);
            UI.DrawMouseText(sB, text, color);
            if (!_tick[indexBool]) {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _tick[indexBool] = true;
            };
        } else { _tick[indexBool] = false; };
    }
};