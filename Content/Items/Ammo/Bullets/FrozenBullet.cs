using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Ammo.Bullets
{
    public class FrozenBullet : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Frozen Ball");
            // Tooltip.SetDefault("");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(97);
            Item.damage = 7;
            Item.shoot = ModContent.ProjectileType<FrozenBulletP>();
            Item.DamageType = DamageClass.Ranged;
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.value = Item.sellPrice(0, 0, 0, 2);
            Item.rare = 2;
            Item.ammo = AmmoID.Bullet;
        }

        public override void AddRecipes()
        {
            CreateRecipe(50)
                .AddIngredient(97, 50)
                .AddIngredient(664, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class FrozenBulletP : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Frozen Ball");
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
            Projectile.light = 0.8f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 1;
            Projectile.scale = 1.1f;
            Projectile.extraUpdates = 2;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(5))
            {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y - 2), 4, 4, 180, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 1.1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.15f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 60, false);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < Main.rand.Next(1, 4); i++)
            {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, 180, Projectile.velocity.X, Projectile.velocity.Y, 50, default, 1.1f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].velocity *= 0.15f;
            }
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }
}