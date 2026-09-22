using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;
using Waybound.Content.Projectiles.Summon.PreHM;

namespace Waybound.Content.Items.Weapons.Summon.PreHM
{
    public class IcebatStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Type] = true;
            ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }

        public override void SetDefaults()
        {
            Item.damage = 18;
            Item.knockBack = 2.5f;
            Item.mana = 10;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(silver: 80);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<IcebatBuff>();
            Item.shoot = ModContent.ProjectileType<IcebatStaffHoldout>();
            Item.channel = true;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.MountedCenter;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            return true;
        }

        private bool HasBuff => Main.LocalPlayer.HasBuff(ModContent.BuffType<IcebatBuff>());
        private Asset<Texture2D> EmptyTexture => ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Summon/PreHM/IcebatStaff_Empty");

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (HasBuff)
            {
                Texture2D tex = EmptyTexture.Value;
                spriteBatch.Draw(tex, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (HasBuff)
            {
                Texture2D tex = EmptyTexture.Value;
                Vector2 origin = tex.Size() * 0.5f;
                spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IceBlock, 25)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class IcebatStaffHoldout : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Summon/PreHM/IcebatStaff";

        private bool HasBuff => Main.player[Projectile.owner].HasBuff(ModContent.BuffType<IcebatBuff>());

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.aiStyle = -1;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<IcebatStaff>() || !player.channel)
            {
                Projectile.Kill();
                return;
            }

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            Vector2 mouse = Main.MouseWorld;
            if (Main.myPlayer != player.whoAmI)
                mouse = player.MountedCenter + player.direction * Vector2.UnitX * 40f;

            Vector2 toMouse = (mouse - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            float targetRot = toMouse.ToRotation();

            player.direction = mouse.X >= player.Center.X ? 1 : -1;
                
            Projectile.rotation = targetRot;
            Projectile.spriteDirection = 1;
            Projectile.Center = player.MountedCenter + toMouse * 14f - Vector2.UnitY * 2f;

            player.itemRotation = targetRot;
            if (player.direction == -1)
                player.itemRotation += MathHelper.Pi;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, targetRot - MathHelper.PiOver2);

            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;

                bool found = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<IcebatProj>())
                    {
                        if (proj.ModProjectile is IcebatProj icebat)
                        {
                            icebat.Stacks++;
                            proj.netUpdate = true;
                        }
                        found = true;
                        break;
                    }
                }

                if (!found && player.whoAmI == Main.myPlayer)
                {
                    int idx = Projectile.NewProjectile(
                        player.GetSource_ItemUse(player.HeldItem),
                        Main.MouseWorld,
                        Vector2.Zero,
                        ModContent.ProjectileType<IcebatProj>(),
                        player.HeldItem.damage,
                        player.HeldItem.knockBack,
                        player.whoAmI
                    );

                    if (idx >= 0 && Main.projectile[idx].ModProjectile is IcebatProj newBat)
                    {
                        newBat.Stacks = 1;
                    }
                }

                SoundEngine.PlaySound(SoundID.Item44, player.Center);
            }

            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            string path = HasBuff
                ? "Waybound/Content/Items/Weapons/Summon/PreHM/IcebatStaff_Empty"
                : "Waybound/Content/Items/Weapons/Summon/PreHM/IcebatStaff";

            Texture2D texture = ModContent.Request<Texture2D>(path).Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects effects = SpriteEffects.None;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0f
            );

            return false;
        }
    }
}