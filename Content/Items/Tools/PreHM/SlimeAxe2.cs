using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Tools.PreHM
{

    public class SlimeAxe2 : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Slime Axe");
			// Tooltip.SetDefault("Caution, do not set fire!");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 7;
			Item.DamageType = DamageClass.Melee;
			Item.axe = 13;
			Item.width = 34;
			Item.height = 30;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = 1;
			Item.knockBack = 1;
			Item.value = Item.sellPrice(0, 0, 48, 0);
			Item.rare = 1;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.shootSpeed = 6;
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.NextBool(5))
			{
				int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 105);
				Main.dust[dust].alpha = 50;
				Main.dust[dust].velocity *= 2;
				Main.dust[dust].scale = Main.rand.NextFloat(1.25f, 1.75f);
				Main.dust[dust].noGravity = true;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ItemID.Gel, 30)
				.AddIngredient(ItemID.GoldBar, 4)
				.AddTile(TileID.Solidifier)
				.Register();
		}
	}
}