namespace Waybound.Tables;

public class Loot(int id, int stack = 1, int chance = 1) : ILootTable {
    public int ItemID { get; private set; } = id;
    public int Stack { get; private set; } = stack;
    public int Сhance { get; private set; } = chance;
    public Loot[] GetLoot() => [this];
}