using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.Items.BossSummon
{
    public class CrystalbringerCore : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Crystalbringer Core");
            // Tooltip.SetDefault("Something must happen if this imitation of heart is inserted into the corpse of a large worm in the crystal depths");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 20;
            Item.value = Item.sellPrice(0, 0, 75, 0);
            Item.rare = ItemRarityID.Pink;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<DeepGolemCore>(1)
                .AddIngredient<DeepCrystalItem>(10)
                .AddIngredient(ItemID.SoulofMight, 1)
                .AddIngredient(ItemID.SoulofSight, 1)
                .AddIngredient(ItemID.SoulofFright, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}