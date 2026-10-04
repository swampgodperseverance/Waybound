using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.NPCs.Bosses.DeepStoneGolem;

namespace Waybound.Content.Items.Weapons.Melee.Flails
{
	public class DeepFlail : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Flail");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.damage = 18;
			Item.width = 42;
			Item.height = 38;
			Item.DamageType = DamageClass.Melee;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6;
			Item.value = Item.sellPrice(0, 0, 50, 0);
            Item.rare = -13;
            Item.master = true;
            Item.UseSound = SoundID.Item1;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<DeepFlailP>();
			Item.shootSpeed = 16;
			Item.channel = true;
			Item.noMelee = true;
			Item.autoReuse = false;
		}
	}
	public class DeepFlailP : Flail
	{
		public override string Texture => "Waybound/Content/Items/Weapons/Melee/Flails/DeepFlailP";
		private bool canSpawn = true;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Needle Flail");
		}

		public override void SetDefaults()
		{
			DrawOriginOffsetX = -3;
			DrawOriginOffsetY = -4;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override void SetStats(ref int throwTime, ref float throwSpeed, ref float recoverDistance, ref float recoverDistance2, ref int attackCooldown)
		{
			throwTime = 10;
			throwSpeed = 17f;
			recoverDistance = 20f;
			recoverDistance2 = 24f;
			attackCooldown = 15;
		}

		public override void ExtraAI()
		{
			Lighting.AddLight(Projectile.position, 0.80f, 0.51f, 0.56f);
			Player player = Main.player[Projectile.owner];
			if (canSpawn && !player.channel && Main.myPlayer == Projectile.owner)
			{
				Vector2 perturbedSpeed = ((new Vector2(Main.screenPosition.X + Main.mouseX, Main.screenPosition.Y + Main.mouseY) - Projectile.Center).SafeNormalize(Vector2.UnitX)).RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(4f, 5f);
				int proj1 = Projectile.NewProjectile(Projectile.InheritSource(Projectile), player.Center, perturbedSpeed, ModContent.ProjectileType<DeepCrystalBall>(), (int)(Projectile.damage * 0.75f), 1, Main.myPlayer);
				Main.projectile[proj1].DamageType = DamageClass.Melee;
				Main.projectile[proj1].timeLeft = 150;
				Main.projectile[proj1].friendly = true;
				Main.projectile[proj1].hostile = false;
				canSpawn = false;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D ballTexture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D ballTextureG = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Melee/Flails/DeepFlailPG").Value;
			Vector2 anchorPos = player.MountedCenter;
			Texture2D chainTexture = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Melee/Flails/DeepFlailPC").Value;
			Texture2D chainTextureG = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Melee/Flails/DeepFlailPCG").Value;
			Vector2 HeadPos = Projectile.Center;
			Rectangle sourceRectangle = new(0, 0, chainTexture.Width, chainTexture.Height);
			Vector2 origin = new(chainTexture.Width * 0.5f, chainTexture.Height * 0.5f);
			float num1 = chainTexture.Height;
			Vector2 vector2_4 = anchorPos - HeadPos;
			var effects = player.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			float rotation = (float)Math.Atan2(vector2_4.Y, vector2_4.X) - 1.57f;
			bool flag = true;
			if (float.IsNaN(HeadPos.X) && float.IsNaN(HeadPos.Y))
				flag = false;
			if (float.IsNaN(vector2_4.X) && float.IsNaN(vector2_4.Y))
				flag = false;
			while (flag)
			{
				if (vector2_4.Length() < num1 + 1.0)
					flag = false;
				else
				{
					Vector2 vector2_1 = vector2_4;
					vector2_1.Normalize();
					HeadPos += vector2_1 * num1;
					vector2_4 = anchorPos - HeadPos;
					Color chainDrawColor = Lighting.GetColor((int)HeadPos.X / 16, (int)(HeadPos.Y / 16f));
					Main.EntitySpriteDraw(chainTexture, HeadPos - Main.screenPosition, new Rectangle?(sourceRectangle), chainDrawColor, rotation, origin, 1, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(chainTextureG, HeadPos - Main.screenPosition, new Rectangle?(sourceRectangle), new Color(255, 168, 135, 0) * 0.2f, rotation, origin, 1, SpriteEffects.None, 0);
				}
			}
			Lighting.AddLight(HeadPos, 0.80f, 0.51f, 0.56f);
			Vector2 position = Projectile.Center - Main.screenPosition;
			Rectangle rect = new(0, 0, ballTexture.Width, ballTexture.Height);
			Vector2 origin2 = new(ballTexture.Width / 2, ballTexture.Height / 2);

			Main.EntitySpriteDraw(ballTexture, position, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
			Main.EntitySpriteDraw(ballTextureG, position, new Rectangle?(rect), Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, Projectile.rotation, origin2, Projectile.scale, effects, 0);
			return false;
		}
	}
}