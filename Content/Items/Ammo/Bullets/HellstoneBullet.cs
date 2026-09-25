using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Ammo.Bullets
{
    public class HellstoneBullet : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hellstone Ball");
            // Tooltip.SetDefault("Sets fire to opponents");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(97);
            Item.damage = 8;
            Item.shoot = ModContent.ProjectileType<HellstoneBulletP>();
            Item.DamageType = DamageClass.Ranged;
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.value = Item.sellPrice(0, 0, 0, 5);
            Item.rare = 3;
            Item.ammo = AmmoID.Bullet;
        }

        public override void AddRecipes()
        {
            CreateRecipe(150)
                .AddIngredient(97, 150)
                .AddIngredient(175, 1)
                .AddTile(77)
                .Register();
        }
    }

    public class HellstoneBulletP : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Hellstone Ball");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Bullet);
            Projectile.width = 12;
            Projectile.height = 2;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.hostile = false;
            Projectile.light = 0.7f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 1;
            Projectile.scale = 1.1f;
            Projectile.extraUpdates = 2;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 4, 4, 6, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 1.1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.15f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 180, false);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 2; i++)
            {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y - 2), 4, 4, 6, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 1f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].velocity *= 0.1f;
            }
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}