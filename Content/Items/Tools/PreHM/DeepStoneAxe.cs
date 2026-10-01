using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.Items.Tools.PreHM
{
	public class DeepStoneAxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Stone Axe");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 8;
			Item.DamageType = DamageClass.Melee;
			Item.axe = 12;
			Item.width = 34;
			Item.height = 30;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = 1;
			Item.knockBack = 1;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.rare = 2;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.shootSpeed = 6;
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