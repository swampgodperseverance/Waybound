using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Items.Accessories.PreHardmode
{
	public class DeepBootsP : ModProjectile
	{
		public override string Texture => "Waybound/Content/Projectiles/EmptyProj";

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Flame of Crystal Gaiters");
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.penetrate = 3;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.scale = 1;
			Projectile.timeLeft = 240;
		}

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(ModContent.BuffType<DeepFire>(), 90, false);
            }
        }

        public override void AI()
		{
			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] >= 15f)
			{
				Projectile.ai[0] = 15f;
				Projectile.velocity.Y = Projectile.velocity.Y + 0.5f;
			}
			if (Projectile.velocity.Y > 20f)
			{
				Projectile.velocity.Y = 20f;
			}

            if (Main.rand.NextBool(5))
            {
                int num1 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<DeepMagicDust>());
                Main.dust[num1].scale = Main.rand.NextFloat(0.25f, 1f);
                Main.dust[num1].alpha = 50;
                Main.dust[num1].velocity = -Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(45)) * Main.rand.NextFloat(1, 1.5f);
                Main.dust[num1].noGravity = false;
            }
        }
	}
}