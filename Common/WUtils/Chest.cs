using Terraria;
using Waybound.Tables;

namespace Waybound.Common.WUtils; 
public class Chest(Item[] inv, int index) {
    public void SetItem(int item, int stack = 1) {
        inv[index].SetDefaults(item);
        inv[index].stack = stack;
        index++;
    }
    public void SetItem(LootTabel loot) {
        foreach (int index in loot.GetLoot().Keys){
            loot.GetLoot().TryGetValue(index, out Loot loot1);
            if (loot1.Сhance == 1) {
                inv[index].SetDefaults(loot1.ItemID);
                inv[index].stack = loot1.Stack;
            }
            else if (WorldGen.genRand.NextBool(1, loot1.Сhance)) {
                inv[index].SetDefaults(loot1.ItemID);
                inv[index].stack = loot1.Stack;
            }
        }
        foreach (int index in loot.GetRandomLoot().Keys) {
            loot.GetRandomLoot().TryGetValue(index, out Loot[] loot1);
            Loot loot2 = WorldGen.genRand.Next(loot1);
            inv[index].SetDefaults(loot2.ItemID);
            inv[index].stack = loot2.Stack;
        }
    }
}