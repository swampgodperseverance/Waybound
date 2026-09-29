using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Armor.Generic.DesertHunter;
using Waybound.Content.Items.Materials.Misc;

namespace Waybound.Content.Items.Armor.Generic.DesertHunterActivated
{
	[AutoloadEquip(EquipType.Legs)]
	public class LeggingsOfDesertHunterActivated : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("3% increased all damage");
			// DisplayName.SetDefault("Leggings of Desert Conqueror");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 18;
			Item.value = Item.sellPrice(0, 0, 35, 0);
			Item.rare = 2;
			Item.defense = 5;
		}

		public override void UpdateEquip(Player player)
		{
			player.moveSpeed += 0.15f;

		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<LeggingsOfDesertHunter>(1)
				.AddIngredient<DesertCore>(1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}