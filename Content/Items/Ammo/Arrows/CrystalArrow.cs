using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;   
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Projectiles.Hostile;
using Waybound.Content.Tiles.Furniture;

namespace Waybound.Content.Items.Ammo.Arrows
{
    public class CrystalArrow : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Deep Crystal Arrow");
            // Tooltip.SetDefault("");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.shoot = ModContent.ProjectileType<crystalArrowP>();
            Item.shootSpeed = 4;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 14;
            Item.height = 36;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.value = Item.sellPrice(0, 0, 0, 2);
            Item.rare = 2;
            Item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            CreateRecipe(20)
                .AddIngredient(40, 20)
                .AddIngredient(ModContent.ItemType<DeepCrystalShardItem>(), 1)
                .AddTile(ModContent.TileType<DeepStoneAltar>())
                .Register();
        }
    }

    public class crystalArrowP : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Deep Crystal Arrow");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 2;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = 1;
            Projectile.scale = 1f;
            Projectile.extraUpdates = 0;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.80f, 0.51f, 0.56f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = lightColor;

            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(new Color(255, 168, 135, 0)) * 0.5f * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture, Projectile.position + drawOrigin, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.position + drawOrigin, null, Projectile.GetAlpha(new Color(3255, 168, 135, 0)) * 0.35f, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < Main.rand.Next(3, 6); i++)
            {
                int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, ModContent.DustType<DeepMagicDust>(), Projectile.velocity.X, Projectile.velocity.Y, 50, default, 1.0f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
                Vector2 perturbedSpeed = new Vector2(4, 0).RotatedByRandom(MathHelper.ToRadians(360)) * Main.rand.NextFloat(0.9f, 1.1f);
                Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 15;
                int proj = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), pos, perturbedSpeed, ModContent.ProjectileType<DeepCrystalProj>(), Projectile.damage / 2, 0, Main.myPlayer);
                Main.projectile[proj].timeLeft = Main.rand.Next(60, 80);
                Main.projectile[proj].scale = Main.rand.NextFloat(0.9f, 1.1f);
            }

            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
        }
    }
}