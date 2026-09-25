using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;

namespace Waybound.Content.Items.Tools.PreHM
{
	public class HielitiumPickaxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Hielitium Pickaxe");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 11;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 100;
			Item.width = 30;
			Item.height = 32;
			Item.scale = 1.25f;
			Item.useTime = 18;
			Item.useAnimation = 23;
			Item.useStyle = 1;
			Item.knockBack = 2;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = 3;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<HielitiumBar>(12)
				.AddIngredient(2503, 8)
				.AddIngredient(5070, 2) //flinx fur
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}