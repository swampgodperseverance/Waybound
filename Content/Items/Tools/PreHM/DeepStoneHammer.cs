using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.Items.Tools.PreHM
{
	public class DeepStoneHammer : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Stone Hammer");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.hammer = 59;
			Item.width = 30;
			Item.height = 30;
			Item.scale = 1;
			Item.useTime = 21;
			Item.useAnimation = 27;
			Item.useStyle = 1;
			Item.knockBack = 5.5f;
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