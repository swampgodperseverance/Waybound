using System.Collections.Generic;
using Waybound.Common.WUtils;

namespace Waybound.Tables;

public class LootTabel(string name) {
    readonly Dictionary<int, Loot> _lootTabel = [];
    readonly Dictionary<int, Loot[]> _randomLoot = [];

    public int LootIndex => _lootIndex;
    int _lootIndex = 0;

    public void SetLoot(ILootTable loot) {
        if (name is null) { throw new System.Exception(Core.DebugLoc.GetLoc("NullTableName")); };
        if (loot.GetLoot().Length == 1) {
            if (!loot.GetLoot()[0].ValidIndex && loot.GetLoot()[0].SlotInChes != -1) { UI.ErrorMsg(string.Format(Core.DebugLoc.GetLoc("TableIndex"), name, loot.GetLoot()[0].SlotInChes > 40 ? Core.DebugLoc.GetLoc("Index40+") : Core.DebugLoc.GetLoc("Index-"))); };
            if (loot.GetLoot()[0].SlotInChes == -1 || !loot.GetLoot()[0].ValidIndex) { _lootTabel.Add(_lootIndex++, loot.GetLoot()[0]); }
            else { _lootTabel.Add(loot.GetLoot()[0].SlotInChes, loot.GetLoot()[0]); }
        }
        else { _randomLoot.Add(_lootIndex++, loot.GetLoot()); }
    }
    public Dictionary<int, Loot> GetLoot() => _lootTabel;
    public Dictionary<int, Loot[]> GetRandomLoot() => _randomLoot;
};