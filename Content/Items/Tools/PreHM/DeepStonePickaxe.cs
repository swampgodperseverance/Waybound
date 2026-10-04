using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.Items.Tools.PreHM
{
	public class DeepStonePickaxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Stone Pickaxe");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 9;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 59;
			Item.width = 30;
			Item.height = 32;
			Item.scale = 1f;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useStyle = 1;
			Item.knockBack = 2;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.rare = 2;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ModContent.ItemType<DeepTreeItem>(), 3)
                .AddIngredient(ModContent.ItemType<DeepCrystalShardItem>(), 3)
                .AddIngredient(ModContent.ItemType<DeepStoneItem>(), 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}