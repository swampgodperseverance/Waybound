using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Waybound.Common.ModSystems.WorldGens;

public class GenMannequin(Item[] inv, Point16 pos, int style = 0, int dire = -1) {
    bool _set = false;

    public void Gen() => WorldGen.PlaceObject(pos.X, pos.Y, TileID.Mannequin, true, style, direction: dire);
    public void Set() {
        if (_set) { return; };
        int teId = TEDisplayDoll.Find(pos.X, pos.Y - 2);
        if (teId == -1) { return; };
        if (TileEntity.ByID.TryGetValue(teId, out TileEntity te)) {
            if (te is TEDisplayDoll doll) {
                Item[] items = (Item[])typeof(TEDisplayDoll).GetField("_items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(doll);
                for (int i = 0; i < inv.Length; i++) { items[i] = inv[i]; }
            };
            _set = true;
        };
    }
    public void SaveWorldData(TagCompound tag, string name) => tag[$"{Waybound.ModName}: set" + name] = _set;
    public void LoadWorldData(TagCompound tag, string name) => _set = tag.GetBool($"{Waybound.ModName}: set" + name);
}