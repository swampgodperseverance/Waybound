using System.Collections.Generic;

namespace Waybound.Tables;

public class LootTabel {
    readonly Dictionary<int, Loot> _lootTabel = [];
    readonly Dictionary<int, Loot[]> _randomLoot = [];

    public int LootIndex => _lootIndex;
    int _lootIndex = 0;

    public void SetLoot(ILootTable loot) {
        if (loot.GetLoot().Length == 1) { _lootTabel.Add(_lootIndex++, loot.GetLoot()[0]); }
        else { _randomLoot.Add(_lootIndex++, loot.GetLoot()); }
    }
    public Dictionary<int, Loot> GetLoot() => _lootTabel;
    public Dictionary<int, Loot[]> GetRandomLoot() => _randomLoot;
};