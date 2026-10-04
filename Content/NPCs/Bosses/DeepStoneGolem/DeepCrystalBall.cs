using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Projectiles.Hostile;

namespace Waybound.Content.NPCs.Bosses.DeepStoneGolem
{
	public class DeepCrystalBall : ModProjectile
	{
		public int counter = 30;
		public int damage;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Crystal Ball");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			DrawOriginOffsetY = -2;
			Projectile.width = 36;
			Projectile.height = 32;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.extraUpdates = 1;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			if (Main.rand.NextBool(2))
			{
				target.AddBuff(ModContent.BuffType<DeepFire>(), 240);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextBool(2))
			{
				target.AddBuff(ModContent.BuffType<DeepFire>(), 150, false);
			}
		}

		public override void AI()
		{
			if (Projectile.friendly)
			{
				damage = Projectile.damage / 2;
			}
			else
			{
				damage = Projectile.damage;
			}

			Projectile.rotation += Projectile.direction * 0.1f;

			Lighting.AddLight(Projectile.Center, 0.80f, 0.51f, 0.56f);

			counter++;

			if (counter >= 60 && Main.myPlayer == Projectile.owner)
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 perturbedSpeed = new Vector2(0, 2.75f).RotatedByRandom(MathHelper.ToRadians(360f));
					int p = Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<DeepCrystalProj>(), damage, 1, Main.myPlayer);
					Main.projectile[p].friendly = Projectile.friendly;
					Main.projectile[p].hostile = Projectile.hostile;
				}
				counter = 0;
			}
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
			for (int i = 0; i < 6; i++)
			{
				Vector2 perturbedSpeed = new Vector2(0, 2.75f).RotatedByRandom(MathHelper.ToRadians(360f));
				int p = Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<DeepCrystalProj>(), damage, 1, Main.myPlayer);
				Main.projectile[p].friendly = Projectile.friendly;
				Main.projectile[p].hostile = Projectile.hostile;
			}
			SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
		}
	}
}