using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.YoYos.GemYoyos
{
	public class AmberYoYo : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Amber Yoyo");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 15;
			Item.DamageType = DamageClass.Melee;
			Item.useTime = 21;
			Item.useAnimation = 21;
			Item.useStyle = 5;
			Item.channel = true;
			Item.knockBack = 2;
			Item.value = Item.sellPrice(0, 0, 40, 0);
			Item.rare = 1;
			Item.autoReuse = false;
			Item.shoot = ModContent.ProjectileType<AmberYoYoP>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(3380, 15)
				.AddIngredient(ItemID.Amber, 8)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class AmberYoYoP : ModProjectile
	{
		public int spawnCounter;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Amber Yoyo");
		}

		public override void SetDefaults()
		{
			Projectile.extraUpdates = 0;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 99;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 6f;
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 160f;
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 11f;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];

			if (spawnCounter++ > 20 && owner.ownedProjectileCounts[ModContent.ProjectileType<AmberYoYoP1>()] < 3)
			{
				Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, new Vector2(1, 1), ModContent.ProjectileType<AmberYoYoP1>(), 0, 0, Main.myPlayer);
			}
		}
	}

	public class AmberYoYoP1 : ModProjectile
	{
		public int spawnCounter;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Amber YoYo");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.extraUpdates = 0;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.minion = false;
			Projectile.alpha = 50;
		}

		public override void OnKill(int timeLeft)
		{
			Color dustColor = Projectile.GetAlpha(new Color(255, 255, 255, 0));

			for (int i = 0; i < 10; i++)
			{
				int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 262, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-10f, 10f), 100, dustColor, Main.rand.NextFloat(0.75f, 1.25f));
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 1;
			}
		}

		public override void AI()
		{
			Color dustColor = Projectile.GetAlpha(new Color(255, 255, 255, 0));

			if (Main.rand.NextBool(5))
			{
				int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 262, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-10f, 10f), 100, dustColor, Main.rand.NextFloat(0.75f, 1.25f));
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 0.2f;
			}

			if (spawnCounter < 1)
			{
				for (int i = 0; i < 10; i++)
				{
					int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 262, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-10f, 10f), 100, dustColor, Main.rand.NextFloat(0.75f, 1.25f));
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity *= 1;
				}
				spawnCounter = 1;
			}

			Player owner = Main.player[Projectile.owner];

			if (!CheckActive(owner))
			{
				return;
			}

			GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
			Shoot();
			Movement(distanceToIdlePosition, vectorToIdlePosition);

			Projectile.rotation = Projectile.velocity.X;
		}

		private bool CheckActive(Player owner)
		{
			if (owner.dead || !owner.active || owner.ownedProjectileCounts[ModContent.ProjectileType<AmberYoYoP>()] == 0)
			{
				Projectile.Kill();
				return false;
			}
			else
			{
				Projectile.timeLeft = 2;
			}

			return true;
		}

		private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
		{
			Vector2 idlePosition = owner.Center;
			idlePosition.Y -= 48f;

			float minionPositionOffsetX = (10 + Projectile.minionPos * 40) * -owner.direction;
			idlePosition.X += minionPositionOffsetX;

			vectorToIdlePosition = idlePosition - Projectile.Center;
			distanceToIdlePosition = vectorToIdlePosition.Length();

			if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f)
			{
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			float overlapVelocity = 0.15f;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];

				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
				{
					if (Projectile.position.X < other.position.X)
					{
						Projectile.velocity.X -= overlapVelocity;
					}
					else
					{
						Projectile.velocity.X += overlapVelocity;
					}

					if (Projectile.position.Y < other.position.Y)
					{
						Projectile.velocity.Y -= overlapVelocity;
					}
					else
					{
						Projectile.velocity.Y += overlapVelocity;
					}
				}
			}
		}
		private void Shoot()
		{
			Projectile.ai[0]++;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC target = Main.npc[i];

				float between = Vector2.Distance(target.Center, Projectile.Center);

				Vector2 perturbedSpeed = (new Vector2(target.position.X + target.width * 0.5f - Projectile.Center.X, target.position.Y + target.height * 0.5f - Projectile.Center.Y)).SafeNormalize(Vector2.UnitX);
				float distance = (float)System.Math.Sqrt((double)(perturbedSpeed.X * perturbedSpeed.X + perturbedSpeed.Y * perturbedSpeed.Y));
				if (between < 350 && !target.friendly && target.active && target.type != 488 && !target.townNPC && Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height))
				{
					if (Projectile.ai[0] > 40)
					{
						distance = 3 / distance;
						perturbedSpeed *= distance * 3;
						int proj2 = Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, perturbedSpeed.RotatedByRandom(MathHelper.ToRadians(10f)), ModContent.ProjectileType<AmberYoYoP2>(), 15, 1, Main.myPlayer);
						Projectile.ai[0] = 0f;
					}
				}
			}
		}

		private void Movement(float distanceToIdlePosition, Vector2 vectorToIdlePosition)
		{
			float speed = 8f;
			float inertia = 20f;

			if (distanceToIdlePosition > 250f)
			{
				if (speed < 80f)
				{
					speed += 0.25f;
				}
				else
				{
					speed = 80f;
				}
				inertia = 40f;
			}
			else
			{
				if (speed > 8)
				{
					speed -= 0.1f;
				}
				else
				{
					speed = 8;
				}
				inertia = 60f;
			}

			if (distanceToIdlePosition > 20f)
			{
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			}
			else if (Projectile.velocity == Vector2.Zero)
			{
				Projectile.velocity.X = -0.15f;
				Projectile.velocity.Y = -0.05f;
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
				Color color = Projectile.GetAlpha(new Color(255, 255, 255, 0)) * 0.75f * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}
			return true;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return new Color(255, 255, 255, 0) * (1f - Projectile.alpha / 255f);
		}
	}
	public class AmberYoYoP2 : ModProjectile
	{
		public override string Texture => "Waybound/Content/Projectiles/EmptyProj";
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Amber YoYo");
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.penetrate = 2;
			Projectile.aiStyle = 0;
			Projectile.hostile = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 180;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override void AI()
		{
			Color dustColor = Projectile.GetAlpha(new Color(255, 255, 255, 0));

			for (int i = 0; i < 1; i++)
			{
				int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 262, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-10f, 10f), 100, dustColor, Main.rand.NextFloat(1f, 1.5f));
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 0.2f;
			}
		}
		public override void OnKill(int timeLeft)
		{
			Color dustColor = Projectile.GetAlpha(new Color(255, 255, 255, 0));

			for (int i = 0; i < 7; i++)
			{
				int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 262, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-10f, 10f), 100, dustColor, Main.rand.NextFloat(1f, 1.5f));
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 1;
			}
		}
	}
}