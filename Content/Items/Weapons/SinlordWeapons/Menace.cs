//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Terraria;
//using Terraria.DataStructures;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Synergia.Common.GlowMasks;
//using Synergia.Content.Buffs;
//using Synergia.Content.Projectiles.Friendly;
//using Synergia.Common.Rarities;

//namespace Synergia.Content.Items.Weapons.Cogworm;

//[AutoloadGlowMask]
//sealed class Menace : ModItem
//{
//    private float recoilRotation;
//    private int recoilTime;
//    private float glowPulse;
//    private float floatTime;
//    private int floatDirection = 1;

//    public override void SetStaticDefaults()
//    {
//        Item.ResearchUnlockCount = 1;
//    }

//    public override void SetDefaults()
//    {
//        Item.staff[Item.type] = true;

//        Item.Size = new Vector2(38, 40);

//        Item.useStyle = ItemUseStyleID.Shoot;
//        Item.useTime = Item.useAnimation = 20;
//        Item.autoReuse = true;

//        Item.DamageType = DamageClass.Magic;
//        Item.damage = 42;
//        Item.knockBack = 2f;

//        Item.noMelee = true;
//        Item.channel = true;
//        Item.mana = 10;

//        Item.rare = ModContent.RarityType<LavaGradientRarity>();

//        Item.UseSound = SoundID.Item105;

//        Item.shoot = ModContent.ProjectileType<MagicStalactite>();
//        Item.shootSpeed = 12f;

//        Item.value = Item.sellPrice(0, 1, 50, 0);
//    }

//    public override Vector2? HoldoutOffset()
//    {
//        floatTime += 0.05f * floatDirection;
//        float offsetY = (float)System.Math.Sin(floatTime) * 2.5f;
//        return new Vector2(0f, -6f + offsetY);
//    }

//    public override void UseStyle(Player player, Rectangle heldItemFrame)
//    {
//        if (player.itemTime == player.itemTimeMax - 1)
//        {
//            recoilRotation = 0.12f * player.direction;
//            recoilTime = 6;
//        }

//        if (recoilTime > 0)
//        {
//            recoilTime--;
//            recoilRotation *= 0.7f;
//        }

//        player.itemRotation += recoilRotation;
//    }

//    public override void HoldItem(Player player)
//    {
//        if (glowPulse > 0f)
//            glowPulse *= 0.88f;
//    }

//    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
//    {
//        Vector2 newVelocity = velocity.SafeNormalize(Vector2.Zero);
//        position += newVelocity * 40;
//        position += new Vector2(-newVelocity.Y, newVelocity.X) * (-10f * player.direction);
//    }

//    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//    {
//        glowPulse = 1f;
//        floatDirection *= -1;

//        Vector2 funnyOffset = Vector2.Normalize(velocity) * 5f;
//        position += funnyOffset - new Vector2(player.direction == -1 ? 0f : 8f, -2f * player.direction).RotatedBy(funnyOffset.ToRotation());

//        for (int i = 0; i < 5; i++)
//        {
//            if (Main.rand.NextBool())
//            {
//                bool flag = Main.rand.NextBool();
//                int dust = Dust.NewDust(position, 0, 0, flag ? 60 : 96, 0, 0.5f, 0, default, (!flag ? 1.5f : 2.3f) + 0.5f * Main.rand.NextFloat());
//                Main.dust[dust].noGravity = true;
//            }
//        }

//        bool hasHellborn = player.HasBuff(ModContent.BuffType<Hellborn>());

//        if (!hasHellborn)
//        {
//            int count = Main.rand.Next(2, 5);

//            for (int i = 0; i < count; i++)
//            {
//                Vector2 spawnPos = new Vector2(
//                    player.Center.X + Main.rand.NextFloat(-60f, 60f),
//                    player.Center.Y + Main.rand.NextFloat(-30f, 20f)
//                );

//                Projectile.NewProjectile(
//                    source,
//                    spawnPos,
//                    velocity,
//                    type,
//                    damage,
//                    knockback,
//                    player.whoAmI
//                );
//            }
//        }
//        else
//        {
//            int bursts = 3;
//            int burstDelay = 5;
//            int shotsPerBurst = 3;

//            for (int burst = 0; burst < bursts; burst++)
//            {
//                for (int shot = 0; shot < shotsPerBurst; shot++)
//                {
//                    Vector2 burstVelocity = velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.9f, 1.1f);

//                    Vector2 spawnPos = new Vector2(
//                        player.Center.X + Main.rand.NextFloat(-80f, 80f),
//                        player.Center.Y + Main.rand.NextFloat(-40f, 30f)
//                    );

//                    Projectile.NewProjectile(
//                        source,
//                        spawnPos,
//                        burstVelocity,
//                        type,
//                        damage,
//                        knockback,
//                        player.whoAmI,
//                        ai0: burst * burstDelay
//                    );
//                }
//            }
//        }

//        return false;
//    }

//    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
//    {
//        Texture2D texture = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
//        Texture2D glow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

//        Vector2 position = Item.position - Main.screenPosition + Item.Size * 0.5f;
//        Rectangle frame = texture.Frame();
//        Vector2 origin = frame.Size() * 0.5f;

//        float pulse = glowPulse * 0.6f;
//        Color outlineColor = Color.White * pulse;

//        for (int i = 0; i < 4; i++)
//        {
//            Vector2 offset = new Vector2(1.5f, 0f).RotatedBy(i * MathHelper.PiOver2);
//            spriteBatch.Draw(glow, position + offset, frame, outlineColor, rotation, origin, scale, SpriteEffects.None, 0f);
//        }

//        spriteBatch.Draw(glow, position, frame, Color.White * (0.4f + glowPulse * 0.6f), rotation, origin, scale, SpriteEffects.None, 0f);
//    }
//}