using System.Collections.Generic;
using Terraria;

namespace Waybound.Common.GloblaItems;

public class OthenItems : GlobalItem {
    public override bool InstancePerEntity => true;

    internal Dictionary<string, Color> lootTooltips = [];

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (lootTooltips.Count <= 0) { return; };
        for (int i = 1; i < tooltips.Count; i++) {
            if (lootTooltips.TryGetValue(tooltips[i].Text, out Color value)) {
                tooltips[i].OverrideColor = value;
            }
        }
    }
};
