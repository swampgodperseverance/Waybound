using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;

namespace Waybound.Content.Items.Tools.PreHM
{
	public class HielitiumHamaxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Hielitium Hamaxe");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 19;
			Item.DamageType = DamageClass.Melee;
			Item.axe = 30;
			Item.hammer = 70;
			Item.width = 32;
			Item.height = 30;
			Item.scale = 1.25f;
			Item.useTime = 14;
			Item.useAnimation = 27;
			Item.useStyle = 1;
			Item.knockBack = 7;
			Item.value = Item.sellPrice(0, 4, 50, 0);
			Item.rare = 3;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<HielitiumBar>(10)
				.AddIngredient(2503, 8)
				.AddIngredient(5070, 2) //flinx fur
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}