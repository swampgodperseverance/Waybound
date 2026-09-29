using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Armor.Generic.DesertHunter
{
	[AutoloadEquip(EquipType.Body)]
	public class BreastplateOfDesertHunter : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Breastplate Of Desert Hunter");
			// Tooltip.SetDefault("2% increased all damage");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 20;
			Item.value = Item.sellPrice(0, 0, 40, 0);
			Item.rare = 1;
			Item.defense = 4;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.05f;
			player.pickSpeed += 0.10f;
		}

		public override void AddRecipes()
		{

		}
	}
}