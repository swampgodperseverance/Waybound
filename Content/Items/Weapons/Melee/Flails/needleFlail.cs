//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using Terraria;
//using Terraria.GameContent;
//using Terraria.GameContent.Creative;
//using Terraria.ID;
//using Terraria.ModLoader;
//using VictimaMod2.Content.Items.Ammo;
//using VictimaMod2.Content.Items.Other;
//using VictimaMod2.Content.Projectiles.Classes;

//namespace VictimaMod2.Content.Items.Weapons.Melee.Flails
//{
//	public class needleFlail : ModItem
//	{

//		public override void SetStaticDefaults()
//		{
//			// DisplayName.SetDefault("Needle Flail");
//			// Tooltip.SetDefault("");
//			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
//		}
//		public override void SetDefaults()
//		{
//			Item.damage = 58;
//			Item.width = 42;
//			Item.height = 38;
//			Item.DamageType = DamageClass.Melee;
//			Item.useTime = 40;
//			Item.useAnimation = 40;
//			Item.useStyle = ItemUseStyleID.Shoot;
//			Item.knockBack = 6;
//			Item.value = Item.sellPrice(0, 0, 50, 0);
//			Item.rare = 4;
//			Item.UseSound = SoundID.Item1;
//			Item.noUseGraphic = true;
//			Item.shoot = ModContent.ProjectileType<needleFlailP>();
//			Item.shootSpeed = 16;
//			Item.channel = true;
//			Item.noMelee = true;
//			Item.autoReuse = false;
//		}
//		public override void AddRecipes()
//		{
//			CreateRecipe(1)
//				.AddIngredient<needleElement>(5)
//				.AddIngredient(1184, 10)
//				.AddTile(TileID.Anvils)
//				.Register();
//			CreateRecipe(1)
//				.AddIngredient<needleElement>(5)
//				.AddIngredient(381, 10)
//				.AddTile(TileID.Anvils)
//				.Register();
//		}
//	}
//	public class needleFlailP : Flail
//	{
//		public override string Texture => "VictimaMod2/Content/Items/Weapons/Melee/Flails/needleFlailP";
//		private bool isChannel = true;
//		private int counter = 0;

//		public override void SetStaticDefaults()
//		{
//			// DisplayName.SetDefault("Needle Flail");
//		}

//		public override void SetDefaults()
//		{
//			Projectile.width = 24;
//			Projectile.height = 24;
//			Projectile.friendly = true;
//			Projectile.penetrate = -1;
//			Projectile.friendly = true;
//			Projectile.DamageType = DamageClass.Melee;
//		}

//		public override void SetStats(ref int throwTime, ref float throwSpeed, ref float recoverDistance, ref float recoverDistance2, ref int attackCooldown)
//		{
//			throwTime = 13;
//			throwSpeed = 20f;
//			recoverDistance = 30f;
//			recoverDistance2 = 34f;
//			attackCooldown = 15;
//		}

//		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
//		{
//			target.AddBuff(BuffID.Venom, 180, false);
//		}

//		public override void ExtraAI()
//		{
//			Player player = Main.player[Projectile.owner];
//			if (!player.channel)
//			{
//				isChannel = false;
//			}

//			if (isChannel == false)
//			{
//				for (int i = 0; i < Main.maxNPCs; i++)
//				{
//					NPC target = Main.npc[i];

//					float between = Vector2.Distance(target.Center, Projectile.Center);

//					if (between < 500 && !target.friendly && target.active && target.type != 488 && !target.townNPC && Main.myPlayer == Projectile.owner)
//					{
//						counter++;
//						if (counter >= 120)
//						{
//							for (int k = 0; k < 6; k++)
//							{
//								Vector2 vel = new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, 3));
//								int proj2 = Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, vel * 2, ModContent.ProjectileType<needleP>(), Projectile.damage / 2, 1, Main.myPlayer);
//								Main.projectile[proj2].DamageType = DamageClass.Melee;
//								Main.projectile[proj2].alpha = 0;
//							}
//							counter = 0;
//						}
//					}
//				}
//			}
//		}

//		public override bool PreDraw(ref Color lightColor)
//		{
//			Player player = Main.player[Projectile.owner];
//			Texture2D ballTexture = TextureAssets.Projectile[Projectile.type].Value;
//			Vector2 anchorPos = player.MountedCenter;
//			Texture2D chainTexture = ModContent.Request<Texture2D>("VictimaMod2/Content/Items/Weapons/Melee/Flails/needleFlailPC").Value;
//			Vector2 HeadPos = Projectile.Center;
//			Rectangle sourceRectangle = new(0, 0, chainTexture.Width, chainTexture.Height);
//			Vector2 origin = new(chainTexture.Width * 0.5f, chainTexture.Height * 0.5f);
//			float num1 = chainTexture.Height;
//			Vector2 vector2_4 = anchorPos - HeadPos;
//			var effects = player.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
//			float rotation = (float)Math.Atan2(vector2_4.Y, vector2_4.X) - 1.57f;
//			bool flag = true;
//			if (float.IsNaN(HeadPos.X) && float.IsNaN(HeadPos.Y))
//				flag = false;
//			if (float.IsNaN(vector2_4.X) && float.IsNaN(vector2_4.Y))
//				flag = false;
//			while (flag)
//			{
//				if (vector2_4.Length() < num1 + 1.0)
//					flag = false;
//				else
//				{
//					Vector2 vector2_1 = vector2_4;
//					vector2_1.Normalize();
//					HeadPos += vector2_1 * num1;
//					vector2_4 = anchorPos - HeadPos;
//					Color chainDrawColor = Lighting.GetColor((int)HeadPos.X / 16, (int)(HeadPos.Y / 16f));
//					Main.EntitySpriteDraw(chainTexture, HeadPos - Main.screenPosition, new Rectangle?(sourceRectangle), chainDrawColor, rotation, origin, 1, SpriteEffects.None, 0);
//				}
//			}
//			Vector2 position = Projectile.Center - Main.screenPosition;
//			Rectangle rect = new(0, 0, ballTexture.Width, ballTexture.Height);
//			Vector2 origin2 = new(ballTexture.Width / 2, ballTexture.Height / 2);

//			Main.EntitySpriteDraw(ballTexture, position, new Rectangle?(rect), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, effects, 0);
//			return false;
//		}
//	}
//}