using System.Collections.Generic;
using Terraria;
using Waybound.Core;

namespace Waybound.Common.GlobalPlayer;

public class GlobalPlayerData : ModPlayer {
    public List<CombatTextData> ActiveTextData { get; private set; } = [];
    public List<string> ActiveTextDataID { get; private set; } = [];

    public override void PostUpdate() {
        bool has = false;
        if (ActiveTextData.Count <= 0) { return; }
        for (int i = 0; i < ActiveTextData.Count; i++) {
            ActiveTextDataID.Add(ActiveTextData[i].ID);
        }
        for (int i = 0; i < 100; i++) {
            if (Main.combatText[i].active) {
                has = true;
                break;
            }
        }
        if (!has) {
            ActiveTextData.Clear();
            ActiveTextDataID.Clear();
        }
    }
};