using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Items.Placeable.Furniture.Aquallite;
using Waybound.Content.Items.Placeable.Furniture.DesertHunterFurniture;

namespace Waybound.Content.Tiles.Furniture.DesertHunterFurniture
{
    public class DesertHunterChest : Abstract.Chest
    {
        public override void SetStaticDefaults()
        {
            DustType = ModContent.DustType<Dusts.DesertHunterDust>();
            // ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<Items.Placeable.Furniture.DesertHunterFurniture.DesertHunterChestI>();
            // ContainerName/* tModPorter Note: Removed. Override DefaultContainerName instead */.SetDefault("Desert Hunter Chest");
            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Desert Hunter Chest");
            AddMapEntry(new Color(120, 171, 191), name, MapChestName);
            RegisterItemDrop(ModContent.ItemType<DesertHunterChestI>());
        }

        public override int GetItemDropType() => ModContent.ItemType<DesertHunterChestI>();

        public static string MapChestName(string name, int i, int j)
        {
            int left = i;
            int top = j;
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX % 36 != 0)
                left--;
            if (tile.TileFrameY != 0)
                top--;

            int chest = Terraria.Chest.FindChest(left, top);
            if (chest < 0)
                return Language.GetTextValue("LegacyChestType.0");

            if (Main.chest[chest].name == "")
                return name;

            return name + ": " + Main.chest[chest].name;
        }
    }
}