using ReLogic.Content;
using Terraria.UI;
using Waybound.Common.Utils;
using Waybound.Content.Race;
using Waybound.Core;
using Waybound.UIs.Elements;

namespace Waybound.UIs;

public class Race(PlayerCreationData data) : UIElement {
    readonly RaceSlot[] raceElement = new RaceSlot[6];
    readonly RaceSlot text = new(data);

    public int posScaleX = 0;
    public int posScaleY = 0;


    public override void OnInitialize() {
        for (int i = 0; i < 6; i++) { raceElement[i] = new(data); }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch) {
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        RaceInfo info = data.race ?? new Human();
        Asset<Texture2D> bigSlotTexture = data.asset ?? asset[10];
        Stat stat = info.GetStat;
        CalculatedStyle dimensions = GetDimensions();
        Vector2 pos = new(dimensions.X + 40 + posScaleX, dimensions.Y + 120 + posScaleY);
        Vector2 textPos = pos.X(94).Y(46);
        Vector2 statPos = pos.X(-18).Y(90);
        int scaleX = 0;

        raceElement[0].DrawSlot(spriteBatch, bigSlotTexture, info, pos.X(20).Y(20), info.RaceName, true);
        for (int i = 1; i < 6; i++) {
            RaceInfo race = CustomClassData.RaceInfo[i - 1];
            raceElement[i].DrawSlot(spriteBatch, asset[9 + i], race, pos.X(130 + scaleX).Y(6), race.RaceName); // 120
            scaleX += 70;
        }
        RaceSlot.DrawText(spriteBatch, info, textPos);
        //text.DrawText(spriteBatch, info, textPos);

        UI.DrawTexture(spriteBatch, asset[4].Value, statPos);
        UI.DrawText(spriteBatch, (100 + stat.BonusHP).ToString(), statPos.X(20).Y(-10));
        UI.DrawTexture(spriteBatch, asset[6].Value, statPos.Y(26));
        UI.DrawText(spriteBatch, (100 + stat.BonusMP).ToString(), statPos.X(20).Y(16));
    }
};