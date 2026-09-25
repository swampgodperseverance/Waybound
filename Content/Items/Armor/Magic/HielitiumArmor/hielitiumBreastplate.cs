using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;

namespace Waybound.Content.Items.Armor.Magic.HielitiumArmor
{
	[AutoloadEquip(EquipType.Body)]
	public class hielitiumBreastplate : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Hielitium Breastplate");
			// Tooltip.SetDefault("10% increased throwing critical strike chance\n7% increased throwing damage");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 24;
			Item.value = Item.buyPrice(0, 0, 60, 0);
			Item.rare = 3;
			Item.defense = 8;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetCritChance(DamageClass.Throwing) += 10;
			player.GetDamage(DamageClass.Throwing) += 0.07f;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<HielitiumBar>(12)
				.AddIngredient(ItemID.Silk, 18)
				.AddIngredient(5070, 6) //flinx fur
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}