using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Armor.Generic.DesertHunter;
using Waybound.Content.Items.Materials.Misc;


namespace Waybound.Content.Items.Armor.Generic.DesertHunterActivated
{
	[AutoloadEquip(EquipType.Body)]
	public class BreastplateOfDesertHunterActivated : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Breastplate of Desert Conqueror");
			// Tooltip.SetDefault("3% increased all damage");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 20;
			Item.value = Item.sellPrice(0, 0, 40, 0);
			Item.rare = 2;
			Item.defense = 8;
		}

		public override void UpdateEquip(Player player)
		{
            player.GetCritChance(DamageClass.Generic) += 0.08f;
        }

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<BreastplateOfDesertHunter>(1)
				.AddIngredient<DesertCore>(1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}