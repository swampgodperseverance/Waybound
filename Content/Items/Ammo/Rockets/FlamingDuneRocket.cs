using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Projectiles;
using Waybound.Particles;

namespace Waybound.Content.Items.Ammo.Rockets
{
	public class FlamingDuneRocket : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Flaming Dune Rocket");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}

		public override void SetDefaults()
		{
            Item.CloneDefaults(ItemID.RocketI);
            Item.width = 14;
			Item.height = 14;
			Item.damage = 20;
			Item.DamageType = DamageClass.Ranged;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(0, 0, 0, 50);
			Item.rare = ItemRarityID.Green;
			Item.shoot = ModContent.ProjectileType<FlamingDuneRocketP>();
			Item.shootSpeed = 10;
			Item.ammo = AmmoID.Rocket;
        }

        public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
        {
            switch (weapon.type)
            {
                case ItemID.RocketLauncher:
                    type = ModContent.ProjectileType<FlamingDuneRocketP>();
                    speed *= 0.4f;
                    break;
                case ItemID.GrenadeLauncher:
                    type = 133;
                    break;
                case ItemID.ProximityMineLauncher:
                    type = 135;
                    break;
                case ItemID.SnowmanCannon:
                    type = 338;
                    break;
                case ItemID.FireworksLauncher: // Celebration
                    type = 167;
                    break;
                case ItemID.ElectrosphereLauncher:
                    type = 442;
                    break;
                case ItemID.Celeb2:
                    type = 714;
                    break;
            }
            base.PickAmmo(weapon, player, ref type, ref speed, ref damage, ref knockback);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            damage = (int)(damage * 0.5f);
            base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
        }

        public override void AddRecipes()
		{
			CreateRecipe(50)
				.AddIngredient<DesertWreckage>(1)
                .AddIngredient<DesertCore>(1)
                .AddTile(TileID.Anvils)
				.Register();
		}
	}

    public class FlamingDuneRocketP : ModProjectile
    {
        public override string Texture => "Waybound/Content/NPCs/Bosses/Themis/ThemisRocket";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Rocket Of Themis");
            ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            DrawOriginOffsetY = -3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.localAI[1] = MathHelper.ToRadians(Main.rand.NextFloat(-90, 90));
            base.OnSpawn(source);
        }

        public override void AI()
        {
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
            Projectile.localAI[0]++;

            if (Projectile.localAI[0] < 10)
                Projectile.velocity = (Projectile.velocity + (Projectile.velocity.RotatedBy(Projectile.localAI[1])).SafeNormalize(Vector2.UnitX) * 1f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
            else if (Projectile.localAI[0] < 35)
                Projectile.velocity = (Projectile.velocity + (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 1.35f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
            Projectile.velocity *= 1.015f;


            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4f, 8f);
                Vector2 vel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.8f, 0.8f);

                ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(18f, 30f)),
                    new Color(255, 140, 40, 220),
                    Main.rand.Next(48, 80)
                ));

                if (Main.rand.NextBool(2))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(14f, 24f)),
                        new Color(255, 160, 50, 200),
                        Main.rand.Next(16, 28)
                    ));
                }
            }
            Lighting.AddLight(Projectile.position, 1.5f, 0.75f, 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DesertExplosion>(), Projectile.damage * 2, 4, Main.myPlayer);
            }
        }
    }
}