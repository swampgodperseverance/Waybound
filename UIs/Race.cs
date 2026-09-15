using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Waybound.Common.WUtils;
using Waybound.Content.Race;
using Waybound.Core;
using Waybound.UIs.Elements;

namespace Waybound.UIs;

public class Race(PlayerCreationData data) : UIElement {
    readonly RaceSlot[] raceElement = new RaceSlot[7];
    readonly bool[] _tick = new bool[2];

    public int posScaleX = 0;
    public int posScaleY = 0;

    public override void OnInitialize() {
        for (int i = 0; i < 7; i++) { raceElement[i] = new(data); }
        for (int i = 0; i < 2; i++) { _tick[i] = false; }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch) {
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        RaceInfo info = data.race ?? new Human();
        Asset<Texture2D> bigSlotTexture = data.asset ?? asset[10];
        Stat stat = info.GetStat;
        CalculatedStyle dimensions = GetDimensions();
        Vector2 pos = new(dimensions.X + 40 + posScaleX, dimensions.Y + 120 + posScaleY);
        Vector2 textPos = pos.X(94).Y(46);
        Vector2 statPos = pos.X(15).Y(117);
        int scaleX = 0;

        UI.DrawTexture(spriteBatch, asset[17].Value, pos.X(209).Y(40)); 

        raceElement[0].DrawSlot(spriteBatch, bigSlotTexture, info, pos.X(20).Y(12), info.RaceName, true);
        for (int i = 1; i < 6; i++) {
            RaceInfo race = CustomClassData.RaceInfo[i - 1];
            raceElement[i].DrawSlot(spriteBatch, asset[9 + i], race, pos.X(130 + scaleX).Y(6), race.RaceName); // 120
            scaleX += 70;
        };
        raceElement[6].DrawText(spriteBatch, info, textPos);

        UI.DrawTexture(spriteBatch, asset[15].Value, statPos);
        UI.DrawText(spriteBatch, (100 + stat.BonusHP).ToString(), statPos.X(-4).Y(-30));
        UI.DrawText(spriteBatch, (20 + stat.BonusMP).ToString(), statPos.X(-4).Y(8));
        DrawStat(spriteBatch, statPos.X(-30).Y(-20), Language.GetText("BestiaryInfo.Life").Value, Color.IndianRed, 4, 0);
        DrawStat(spriteBatch, statPos.X(-30).Y(14), Language.GetText("LegacyInterface.2").Value, Color.CornflowerBlue, 6, 1);
    }
    void DrawStat(SpriteBatch sB, Vector2 pos, string text, Color color, int indexTexture, int indexBool) {
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        UI.DrawTexture(sB, asset[indexTexture].Value, pos);
        if (UI.Hover(pos, asset[indexTexture].Value)) {
            UI.DrawTexture(sB, asset[indexTexture + 1].Value, pos);
            UI.DrawMouseText(sB, text, color);
            if (!_tick[indexBool]) {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _tick[indexBool] = true;
            };
        } else { _tick[indexBool] = false; };
    }
};