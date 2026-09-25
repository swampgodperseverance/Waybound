using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;
using static Terraria.ModLoader.ModContent;

namespace Waybound.Content.Items.Armor.Magic.HielitiumArmor
{
	[AutoloadEquip(EquipType.Head)]
	public class hielitiumHelmet : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("9% increased throwing damage");
			// DisplayName.SetDefault("Hielitium Hood");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 24;
			Item.value = Item.buyPrice(0, 0, 90, 0);
			Item.rare = 3;
			Item.defense = 7;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Throwing) += 0.09f;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<hielitiumBreastplate>() && legs.type == ItemType<hielitiumLeggings>();
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "After taking damage, the armor can create an ice coating that gives 8 defense for 3 seconds\nYour stamina bar has increased by 7";

		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<HielitiumBar>(4)
				.AddIngredient(ItemID.Silk, 10)
				.AddIngredient(5070, 2) //flinx fur
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}