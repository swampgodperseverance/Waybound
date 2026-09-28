using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Armor.Generic.DesertHunter
{
	[AutoloadEquip(EquipType.Legs)]
	public class LeggingsOfDesertHunter : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("2% increased all damage");
			// DisplayName.SetDefault("Leggings Of Desert Hunter");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 18;
			Item.value = Item.sellPrice(0, 0, 35, 0);
			Item.rare = 1;
			Item.defense = 3;
		}

		public override void UpdateEquip(Player player)
		{
			player.moveSpeed += 0.10f;
		}

		public override void AddRecipes()
		{
		
		}
	}
}