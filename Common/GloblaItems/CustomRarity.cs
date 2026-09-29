using Terraria;
using Waybound.Common.Rarities;

namespace Waybound.Common.GloblaItems;

public class CustomRarity : GlobalItem {
    public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line) {
        if (RarityLoader.GetRarity(item.rare) is WayboundRarity rarity) { rarity.Draw(item, line); }
    }
};
