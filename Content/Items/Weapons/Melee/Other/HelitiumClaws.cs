using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Bars;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Other
{
	public class HielitiumClaws : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Hielitium Claws");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			Item.claw[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 13;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 36;
			Item.useTime = 7;
			Item.useAnimation = 7;
			Item.useStyle = 1;
			Item.knockBack = 1;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.shootSpeed = 13f;
			Item.shoot = ModContent.ProjectileType<HielitiumClawsP>();
			Item.rare = 3;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{

		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<HielitiumBar>(8)
				.AddIngredient(2503, 8)
				.AddIngredient(5070, 2) //flinx fur
				.AddTile(TileID.Anvils)
				.Register();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(25f));
			newVelocity *= Main.rand.NextFloat(1f, 1.5f);
			Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
			return false;
		}
	}

    public class HielitiumClawsP : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Other/HielitiumClawsP";

        private Vector2 oldPos = Vector2.Zero;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 8;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.extraUpdates = 0;
            Projectile.scale = 1f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.45f);

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), DustID.IceTorch, Projectile.velocity * 0.15f, 80, new Color(140, 210, 255), 0.9f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.55f, 0.85f);
        }

        private void SpawnShortFlash()
        {
            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: Projectile.Center.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(Main.rand.NextFloat(55f, 75f)),
                color: new Color(200, 235, 255, 255),
                duration: 10
            ));
            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: Projectile.Center.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(Main.rand.NextFloat(35f, 50f)),
                color: new Color(160, 215, 255, 255),
                duration: 11
            ));
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            SpawnShortFlash();

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4.5f, 4.5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, vel, 40, new Color(120, 200, 255), Main.rand.NextFloat(1.1f, 1.7f));
                d.noGravity = true;
            }

            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Frost, Main.rand.NextVector2Circular(2.5f, 2.5f), 80, new Color(180, 230, 255), 1.1f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.55f, Pitch = 0.25f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = lightColor;

            if (oldPos != Vector2.Zero && oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                Color trailColor = new Color(70, 170, 255, 0) * 0.55f;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 2.2f;

                Main.EntitySpriteDraw(
                    trailTex,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.55f, trailScaleY),
                    SpriteEffects.None,
                    0f
                );

                Main.EntitySpriteDraw(
                    trailTex,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor * 0.4f,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.3f, trailScaleY * 1.15f),
                    SpriteEffects.None,
                    0f
                );
            }

            Color outline = new Color(90, 180, 255) * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0f).RotatedBy(MathHelper.TwoPi / 4f * i);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}