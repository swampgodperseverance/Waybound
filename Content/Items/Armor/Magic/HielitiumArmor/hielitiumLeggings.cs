using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;

namespace Waybound.Content.Items.Armor.Magic.HielitiumArmor
{
	[AutoloadEquip(EquipType.Legs)]
	public class hielitiumLeggings : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("12% increased the chance that you won't consume thrown weapon\n8% increased move speed");
			// DisplayName.SetDefault("Hielitium Leggings");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 18;
			Item.value = Item.buyPrice(0, 0, 60, 0);
			Item.rare = 3;
			Item.defense = 5;
		}

		public override void UpdateEquip(Player player)
		{
			player.moveSpeed += 0.08f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<HielitiumBar>(8)
				.AddIngredient(ItemID.Silk, 14)
				.AddIngredient(5070, 4) //flinx fur
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}