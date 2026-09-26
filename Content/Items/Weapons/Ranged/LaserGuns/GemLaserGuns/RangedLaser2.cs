using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns
{
	public abstract class RangedLaser2 : ModProjectile
	{
		public float moveDistance = 90f;
		public int maxDistance = 250;
		public float rotation = 0;
		public int laserDust;

		public virtual void SetStats(ref float moveDistance, ref int maxDistance, ref float rotation, ref int laserDust)
		{
		}

		public float Distance
		{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public void DrawLaser(Texture2D texture, Vector2 start, Vector2 unit, float step, int damage, float rotation = 0f, float scale = 1f, float maxDist = 2000f, Color color = default(Color), int transDist = 50)
		{
			float r = unit.ToRotation() + rotation;

			for (float i = transDist; i <= Distance; i += step)
			{
				Color c = Color.White;
				var origin = start + i * unit;
				Main.EntitySpriteDraw(texture, origin - Main.screenPosition,
					new Rectangle(0, 26, 28, 26), i < transDist ? Color.Transparent : c, r, new Vector2(28 * 0.5f, 26 * 0.5f), scale, 0, 0);
			}

			Main.EntitySpriteDraw(texture, start + unit * (transDist - step) - Main.screenPosition,
				new Rectangle(0, 0, 28, 26), Color.White, r, new Vector2(28 * 0.5f, 26 * 0.5f), scale, 0, 0);

			Main.EntitySpriteDraw(texture, start + (Distance + step) * unit - Main.screenPosition,
				new Rectangle(0, 52, 28, 26), Color.White, r, new Vector2(28 * 0.5f, 26 * 0.5f), scale, 0, 0);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Player player = Main.player[Projectile.owner];
			Vector2 unit = Projectile.velocity;
			float point = 1f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), player.Center,
				player.Center + unit * Distance, 22, ref point);
		}

		public override void AI()
		{
			SetStats(ref moveDistance, ref maxDistance, ref rotation, ref laserDust);

			Player player = Main.player[Projectile.owner];

			Projectile.position = player.Center + Projectile.velocity * moveDistance;

			UpdatePlayer(player);
			SetLaserPosition(player);
			SpawnDusts(player);
			CastLights();
		}

		private void CastLights()
		{
			DelegateMethods.v3_1 = new Vector3(0.8f, 0.8f, 0.8f);
			Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * (Distance - moveDistance), 26, DelegateMethods.CastLight);
		}

		private void SetLaserPosition(Player player)
		{
			for (Distance = moveDistance; Distance <= maxDistance; Distance += 5f)
			{
				var start = player.Center + Projectile.velocity * Distance;
				if (!Collision.CanHitLine(player.Center, 1, 1, start, 1, 1) && !Collision.CanHit(player.Center, 1, 1, start, 1, 1))
				{
					Distance += 20;
					break;
				}
			}
		}

		private void SpawnDusts(Player player)
		{
			Vector2 unit = Projectile.velocity * -1;
			Vector2 dustPos = player.Center + Projectile.velocity * (Distance - 20);

			for (int i = 0; i < 2; ++i)
			{
				float num1 = Projectile.velocity.ToRotation() + (Main.rand.Next(2) == 1 ? -1.0f : 1.0f) * 1.57f;
				float num2 = (float)(Main.rand.NextDouble() * 0.8f + 1.0f);
				Vector2 dustVel = new Vector2((float)Math.Cos(num1) * num2, (float)Math.Sin(num1) * num2);
				int dust = Dust.NewDust(dustPos, 0, 0, laserDust, dustVel.X, dustVel.Y);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].scale = 1.2f;
			}

			if (Main.rand.NextBool(3))
			{
				Vector2 offset = Projectile.velocity.RotatedBy(1.57f) * ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;
				int dust = Dust.NewDust(dustPos + offset - Vector2.One * 4f, 8, 8, 31, 0.0f, 0.0f, 100, new Color(), 1.5f);
				Main.dust[dust].velocity *= 0.5f;
				Main.dust[dust].velocity.Y = -Math.Abs(Main.dust[dust].velocity.Y);
				unit = dustPos - Main.player[Projectile.owner].Center;
				unit.Normalize();
			}

			Vector2 offset2 = Projectile.velocity;
			offset2 *= moveDistance - 55;
			Vector2 pos = player.Center + offset2 - new Vector2(10, 10);
			Vector2 dustVelocity = Vector2.UnitX * 18f;
			dustVelocity = dustVelocity.RotatedBy(Projectile.rotation - 1.57f);
			Vector2 spawnPos = Projectile.Center + dustVelocity;
			for (int k = 0; k < 2; k++)
			{
				Vector2 spawn = spawnPos + ((float)Main.rand.NextDouble() * 6.28f).ToRotationVector2() * (12f);
				int dust = Dust.NewDust(pos, 20, 20, laserDust, Projectile.velocity.X / 2f, Projectile.velocity.Y / 2f);
				Main.dust[dust].velocity = Vector2.Normalize(spawnPos - spawn) * 1.5f * (10f) / 10f;
				Main.dust[dust].noGravity = true;
				Main.dust[dust].scale = Main.rand.Next(10, 20) * 0.05f;
			}
		}

		private void UpdatePlayer(Player player)
		{
			if (Projectile.owner == Main.myPlayer)
			{
				Vector2 diff = Main.MouseWorld - player.Center;
				diff.Normalize();
				Projectile.velocity = diff;
				Projectile.direction = Main.MouseWorld.X > player.position.X ? 1 : -1;
				Projectile.netUpdate = true;
			}
			int dir = Projectile.direction;
			player.ChangeDir(dir);

			player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * dir, Projectile.velocity.X * dir);
		}

		public override bool ShouldUpdatePosition() => false;

		public override void CutTiles()
		{
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 unit = Projectile.velocity;
			Utils.PlotTileLine(Projectile.Center, Projectile.Center + unit * Distance, (Projectile.width + 16) * Projectile.scale, DelegateMethods.CutTiles);
		}
	}
}