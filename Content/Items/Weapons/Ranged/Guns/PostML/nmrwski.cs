using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Waybound.Content.Items.Weapons.Ranged.Guns.PostML
{
    public class nmrwski : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 212;
            Item.height = 102;

            Item.damage = 350;          
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 45;           
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 15f;       
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ItemRarityID.Red;

            Item.noUseGraphic = true;
            Item.channel = true;

            Item.UseSound = SoundID.Item38; 

            Item.shoot = ModContent.ProjectileType<nmrwskiProj>();
            Item.shootSpeed = 45f;     
            Item.useAmmo = AmmoID.Bullet;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<nmrwskiHeldProj>(), damage, knockback, player.whoAmI);
            return false; 
        }
    }
    public class nmrwskiProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = ProjAIStyleID.Boulder;  
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;               
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;                
            Projectile.extraUpdates = 4;            
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Projectile.velocity * 0.2f, 100, Color.Orange, 1.5f);
                dust.noGravity = true;
            }
        }
    }

    public class nmrwskiHeldProj : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Guns/PostML/nmrwski"; 

        public override void SetDefaults()
        {
            Projectile.width = 212;
            Projectile.height = 102;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = player.MountedCenter;
            Projectile.timeLeft = 2;

            Vector2 mousePos = Main.MouseWorld;
            Vector2 direction = mousePos - Projectile.Center;
            direction.Normalize();

            Projectile.velocity = direction;
            Projectile.rotation = direction.ToRotation();

            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation += MathHelper.Pi;
            }

            player.ChangeDir(mousePos.X < player.Center.X ? -1 : 1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            if (Projectile.ai[0] == 0)
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    Vector2 shootVelocity = direction * 45f;
                    int bulletType = ModContent.ProjectileType<nmrwskiProj>();

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + direction * 60f, shootVelocity, bulletType, Projectile.damage, Projectile.knockBack, player.whoAmI);


                    Vector2 recoil = -direction * 28f;

                    player.velocity = recoil;
                }
                Projectile.ai[0] = 1; 
            }
        }
    }
}