using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Projectiles.Hostile
{
	public class DeepCrystalProj : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Crystal Shard");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 2;
			Projectile.extraUpdates = 2;
			Projectile.timeLeft = 90;
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Generic;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			if (Main.rand.NextBool(2))
			{
				target.AddBuff(ModContent.BuffType<DeepFire>(), 180);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextBool(2))
			{
				target.AddBuff(ModContent.BuffType<DeepFire>(), 90, false);
			}
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
			Lighting.AddLight(Projectile.Center, 0.80f, 0.51f, 0.56f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.5f * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}

			Vector2 pos = (Projectile.position - Main.screenPosition) + drawOrigin + new Vector2(0, 0);
			Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(lightColor) * 1, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(texture, pos, null, Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, ModContent.DustType<DeepMagicDust>(), Projectile.velocity.X, Projectile.velocity.Y, 50, default(Color), 1.1f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 0.5f;
			}
			SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
		}
	}
}