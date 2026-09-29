using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Misc;

namespace Waybound.Content.Items.Accessories.Inventory
{
	public class Antiradar : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Antiradar");
            /* Tooltip.SetDefault("You can't be detected by Themis\n" +
                "Works in inventory"); */
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 15;
			Item.height = 15;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}

		public override void UpdateInventory(Player player)
		{
            player.GetModPlayer<AntiradarPlayer>().antiradar = 1;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
            player.GetModPlayer<AntiradarPlayer>().antiradar = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<DesertWreckage>(4)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class AntiradarPlayer : ModPlayer
    {
        public int antiradar;

        public float dam;

        public override void ResetEffects()
        {

            antiradar = 0;
        }

        public override void PreUpdate()
        {
            if (Main.masterMode)
            {
                dam = 1.666f;
            }
            else if (Main.expertMode && !Main.masterMode)
            {
                dam = 1.333f;
            }
            else
            {
                dam = 1f;
            }

            base.PreUpdate();
        }

    }
}