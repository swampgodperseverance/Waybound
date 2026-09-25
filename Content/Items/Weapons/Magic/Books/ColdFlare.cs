//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using Terraria;
//using Terraria.Audio;
//using Terraria.DataStructures;
//using Terraria.GameContent;
//using Terraria.GameContent.Creative;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Waybound.Content.Items.Placeable.Bars;
//using static Terraria.ModLoader.ModContent;

//namespace Waybound.Content.Items.Weapons.Magic.Books
//{
//	public class ColdFlare : ModItem
//	{
//		public override void SetStaticDefaults()
//		{
//			// Tooltip.SetDefault("");
//			// DisplayName.SetDefault("Cold Flare");
//			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
//		}

//		public override void SetDefaults()
//		{
//			Item.damage = 16;
//			Item.DamageType = DamageClass.Magic;
//			Item.width = 28;
//			Item.height = 30;
//			Item.useTime = 16;
//			Item.useAnimation = 16;
//			Item.useStyle = 5;
//			Item.noMelee = true;
//			Item.knockBack = 3f;
//			Item.value = Item.sellPrice(0, 2, 0, 0);
//			Item.rare = 3;
//			Item.mana = 14;
//			Item.UseSound = SoundID.Item43;
//			Item.autoReuse = true;
//			Item.shoot = ProjectileType<ColdFlareP>();
//			Item.shootSpeed = 7.5f;
//		}

//		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//		{
//			for (int i = 0; i < 3; i++)
//			{
//				Vector2 vel = velocity.RotatedBy(MathHelper.ToRadians(-25 + 25 * i));
//				int proj1 = Projectile.NewProjectile(source, position, vel, type, damage, knockback, player.whoAmI);
//				Main.projectile[proj1].DamageType = DamageClass.Magic;
//			}
//			return false;
//		}

//		public override void AddRecipes()
//		{
//			CreateRecipe(1)
//				.AddIngredient(165, 1)
//				.AddIngredient<iceRain>(1)
//				.AddIngredient<hielitiumBar>(7)
//				.AddIngredient(5070, 2)
//				.AddTile(TileID.Bookcases)
//				.Register();
//		}
//	}

//	public class ColdFlareP : ModProjectile
//	{
//		public override void SetStaticDefaults()
//		{
//			// DisplayName.SetDefault("Cold Flare");
//			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
//			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
//		}

//		public override void SetDefaults()
//		{
//			Projectile.width = 16;
//			Projectile.height = 16;
//			Projectile.friendly = true;
//			Projectile.usesLocalNPCImmunity = true;
//			Projectile.localNPCHitCooldown = 6;
//			Projectile.penetrate = 4;
//			Projectile.hostile = false;
//			Projectile.DamageType = DamageClass.Magic;
//			Projectile.tileCollide = true;
//			Projectile.ignoreWater = true;
//		}

//		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
//		{
//			target.AddBuff(BuffID.Frostburn, 120, false);
//		}

//		public override void AI()
//		{
//			Lighting.AddLight(Projectile.Center, 0.30f, 0.55f, 0.70f);
//			Projectile.rotation += Projectile.direction * 0.25f;

//			if (Main.rand.NextBool(3))
//			{
//				int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 180, Projectile.velocity.X, Projectile.velocity.Y, 50, default(Color), 1.50f);
//				Main.dust[dust].noGravity = true;
//				Main.dust[dust].velocity *= 0.5f;
//			}
//		}

//		public override bool OnTileCollide(Vector2 oldVelocity)
//		{
//			Projectile.penetrate -= 1;
//			if (Projectile.penetrate <= 0)
//			{
//				Projectile.Kill();
//			}
//			else
//			{
//				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
//				if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
//					Projectile.velocity.X = -oldVelocity.X;
//				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
//					Projectile.velocity.Y = -oldVelocity.Y;
//				Projectile.velocity /= 1.25f;
//				SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
//			}

//			return false;
//		}

//		public override void OnKill(int timeLeft)
//		{
//			SoundEngine.PlaySound(SoundID.Item27, Projectile.position);

//			for (int i = 0; i < 10; i++)
//			{
//				int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 180, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 50, default(Color), 1.50f);
//				Main.dust[dust].noGravity = true;
//				Main.dust[dust].velocity *= Main.rand.NextFloat(1, 2);
//			}
//		}

//		public override Color? GetAlpha(Color lightColor)
//		{
//			return new Color(118, 218, 244, 0) * (1f - Projectile.alpha / 255f);
//		}

//		public override bool PreDraw(ref Color lightColor)
//		{
//			Color drawColor = lightColor;

//			Main.instance.LoadProjectile(Projectile.type);
//			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

//			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
//			for (int k = 0; k < Projectile.oldPos.Length; k++)
//			{
//				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
//				Color color = Projectile.GetAlpha(new Color(118, 218, 244, 0)) * (1f - Projectile.alpha / 255f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
//				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
//			}
//			return true;
//		}
//	}
//}