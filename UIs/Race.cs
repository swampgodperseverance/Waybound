using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReLogic.Content;
using Terraria.UI;
using Waybound.Common.Utils;
using Waybound.Content.Race;
using Waybound.Core;
using Waybound.UIs.Elements;

namespace Waybound.UIs;

public class Race(PlayerCreationData data) : UIElement {
    readonly RaceSlot[] raceElement = new RaceSlot[6]; 

    public override void OnInitialize() {
        for (int i = 0; i < 6; i++) { raceElement[i] = new(data); }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch) {
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        RaceInfo info = data.race ?? new Human();
        Stat stat = info.GetStat;
        CalculatedStyle dimensions = GetDimensions();
        Vector2 pos = new(dimensions.X + 40, dimensions.Y + 120);
        Vector2 textPos = pos.X(100).Y(30);
        Vector2 statPos = pos.X(-18).Y(90);
        string raceInfo = info.Info;
        int scaleX = 0;
        int index = 0;
        int count = 1;

        for (int i = 0; i < raceInfo.Length; i++) {
            if (index == 39) {
                int scale = 0;
                if (raceInfo[index * count] == ' ') { raceInfo = raceInfo.Remove(index * count, 1); }
                else { raceInfo = raceInfo.Insert(index * count, "-"); scale = 1; };
                raceInfo = raceInfo.Insert(index * count + scale, "\n");
                index = 0;
                count++;
            };
            index++;
        };
        raceElement[0].DrawSlot(spriteBatch, asset[4], info, pos.X(20).Y(20), info.RaceName, true);
        for (int i = 1; i < 6; i++) {
            RaceInfo race = CustomClassData.RaceInfo[i - 1];
            raceElement[i].DrawSlot(spriteBatch, asset[4], race, pos.X(120 + scaleX).Y(-6), race.RaceName);
            scaleX += 70;
        }

        UI.DrawText(spriteBatch, raceInfo, textPos);
        UI.DrawTexture(spriteBatch, asset[4].Value, statPos);
        UI.DrawText(spriteBatch, (100 + stat.BonusHP).ToString(), statPos.X(20).Y(-10));
        UI.DrawTexture(spriteBatch, asset[6].Value, statPos.Y(26));
        UI.DrawText(spriteBatch, (100 + stat.BonusMP).ToString(), statPos.X(20).Y(16));
    }
};