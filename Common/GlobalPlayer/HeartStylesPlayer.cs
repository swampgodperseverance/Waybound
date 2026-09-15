using Waybound.Common.HeartStyles;
using Waybound.Common.WUtils;

namespace Waybound.Common.GlobalPlayer;

public class HeartStylesPlayer : ModPlayer {
    private HeartStyle _activeStyle = new Default();

    public int MaxCount => Player.ConsumedLifeCrystals + 5;
    public int CurrentHeart {
        get {
            int currentHP = Player.ConsumedLifeCrystals + 5;
            float scale = UI.GetProgress(currentHP, 15 + currentHP < 5 ? 5 : 0);
            int heartScale = (int)((Player.statLifeMax2 * scale) / (currentHP));
            return Player.statLife / heartScale;
        }
    }

    public HeartStyle ActiveStyle => _activeStyle;

    public override void Initialize() => _activeStyle = new Default();
    public override void ResetEffects() => _activeStyle = new Default();

    public HeartStyle GetStyleForHeart(int index, string activeStyleName, HeartStyle exclude = null) {
        HeartStyle result = null;
        int resultPriority = int.MaxValue;

        foreach (HeartStyle style in Core.CustomClassData.Heart) {
            if (style == exclude) { continue; }
            if (!style.Active(Player, activeStyleName)) { continue; };

            int scale = 0;
            int scale2 = 0;
            if (activeStyleName == "HorizontalBarsWithFullText" || activeStyleName == "HorizontalBarsWithText" || activeStyleName == "HorizontalBars") {
                scale = 1;
                if ((Race.GetID(Player.GetModPlayer<RacePlayer>().Race) == Race.ID.Viking || Race.GetID(Player.GetModPlayer<RacePlayer>().Race) == Race.ID.Dwarf) && Player.ConsumedLifeFruit == 0) { scale2 = 2; };
            }
            if (activeStyleName == "Default" && (Race.GetID(Player.GetModPlayer<RacePlayer>().Race) == Race.ID.Viking || Race.GetID(Player.GetModPlayer<RacePlayer>().Race) == Race.ID.Dwarf) && Player.ConsumedLifeFruit == 0) { scale2 = -1; }

            int count = style.Count(Player);
            if (count <= 0) { continue; };

            bool contains;

            if (style.Flip) { contains = index >= MaxCount - count + scale && index <= MaxCount + scale2; }
            else { contains = index >= 0 && index < count; };
            if (!contains) { continue; };
            if (result == null || style.Priority(Player) < resultPriority) {
                result = style;
                _activeStyle = result;
                resultPriority = style.Priority(Player);
            };
        };
        return result;
    }
}