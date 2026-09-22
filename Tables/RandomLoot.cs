namespace Waybound.Tables; 
public class RandomLoot(params Loot[] loot) : ILootTable {
    public Loot[] GetLoot() => loot;
}