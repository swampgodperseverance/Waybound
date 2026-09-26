using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns
{
	public class SapphireLaserGun : LaserGun
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("");
			// DisplayName.SetDefault("Sapphire Laser Gun");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 15;
			Item.crit = -4;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 2;
			Item.useAnimation = 2;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.knockBack = 0f;
			Item.value = Item.sellPrice(0, 0, 4, 0);
			Item.rare = 1;
			Item.UseSound = SoundID.Item12;
			Item.autoReuse = false;
			Item.shoot = ProjectileType<SapphireLaserGunP>();
			Item.shootSpeed = 1f;
			chargeMax = 195;
			chargeAdd = 1;
			chargeRemove = 4;
		}
        public override bool CanUseItem(Player player) =>
			player.ownedProjectileCounts[Item.shoot] < 1;

        public override Vector2? HoldoutOffset()
		{
			return new Vector2(-8, 1);
        }
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            spriteBatch.Draw(Terraria.GameContent.TextureAssets.Item[Item.type].Value, position, frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0);
            return false;
        }

        public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ItemID.SilverBar, 10)
				.AddIngredient(ItemID.Sapphire, 8)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class SapphireLaserGunP : RangedLaser
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Amethyst Laser Gun");
		}

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.scale = 0.3f;
			Projectile.timeLeft = 2;
			moveDistance = 37f;
            moveSpeed = 2.5f;
            maxDistance = 300;
			laserDust = DustType<SapphireLaserGunD>();
            colorLineBG = new(35, 153, 255, 100);
            colorLinesAround = new(122, 232, 255, 100);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            DrawLaser(owner.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * moveDistance, Projectile.velocity, -MathHelper.PiOver2);
            return false;
        }
    }

	public class SapphireLaserGunD : ModDust
	{
		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return Color.White;
		}
	}
}