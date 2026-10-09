using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Accessories.PreHardmode;

namespace Waybound.Content.Items.Accessories.Hardmode
{
    public class BeltoftheDeathbringer : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
            Item.accessory = true;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(gold: 25);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            MBPlayer mp = player.GetModPlayer<MBPlayer>();
            mp.equipped = true;
            mp.deathbringer = true;

            player.GetDamage(DamageClass.Ranged) += 0.12f;
            player.GetCritChance(DamageClass.Ranged) += 8f;
        }

        public override void PostUpdate()
        {
            float pulse = 0.75f + 0.25f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + Item.whoAmI);
            Lighting.AddLight(Item.Center, new Vector3(0.7f, 0.12f, 0.2f) * 0.45f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.7f + 0.3f * MathF.Sin(time * 3f + whoAmI);
            Color glow = new Color(235, 45, 70, 0);

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(2.5f, 0f).RotatedBy(time * 1.2f + MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, position + off, null, glow * (0.28f * pulse), rotation, origin, scale * 1.03f, SpriteEffects.None, 0f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<MegaBullet>()
                .AddIngredient(ItemID.LunarBar, 14)
                .AddIngredient(ItemID.FragmentVortex, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class DeathbringerNPC : GlobalNPC
    {
        public int markTimer;
        public int markOwner = -1;

        public override bool InstancePerEntity => true;

        public void Mark(int owner)
        {
            markTimer = 300;
            markOwner = owner;
        }

        public override void PostAI(NPC npc)
        {
            if (markTimer <= 0) return;

            markTimer--;

            if (Main.netMode == NetmodeID.Server) return;

            Lighting.AddLight(npc.Center, new Vector3(0.6f, 0.1f, 0.18f));

            if (Main.GameUpdateCount % 3 != 0) return;

            float radius = Math.Max(npc.width, npc.height) * 0.5f + 16f;
            float ang = Main.GlobalTimeWrappedHourly * 4f;
            for (int k = 0; k < 2; k++)
            {
                Vector2 pos = npc.Center + Vector2.UnitX.RotatedBy(ang + MathHelper.Pi * k) * radius;
                Dust dust = Dust.NewDustPerfect(pos, DustID.RedTorch, Vector2.Zero, 100, default, 1f);
                dust.noGravity = true;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (markTimer <= 0 || projectile.owner != markOwner) return;
            if (!projectile.CountsAsClass(DamageClass.Ranged)) return;
            if (!Main.player[projectile.owner].GetModPlayer<MBPlayer>().deathbringer) return;

            modifiers.FinalDamage *= 1.15f;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (markTimer <= 0) return;

            Texture2D ray = TextureAssets.Extra[98].Value;
            Vector2 rayOrigin = new Vector2(ray.Width / 2f, ray.Height);
            Vector2 center = npc.Center - screenPos;
            float time = Main.GlobalTimeWrappedHourly;
            float fade = Math.Min(1f, markTimer / 30f);
            float radius = Math.Max(npc.width, npc.height) * 0.5f + 12f + 3f * MathF.Sin(time * 8f);
            Color blood = new Color(235, 45, 70, 0);

            for (int i = 0; i < 4; i++)
            {
                float ang = MathHelper.PiOver2 * i + time * 1.5f;
                Vector2 pos = center + Vector2.UnitX.RotatedBy(ang) * radius;
                spriteBatch.Draw(ray, pos, null, blood * (0.7f * fade), ang + MathHelper.PiOver2, rayOrigin, new Vector2(0.1f, 0.22f), SpriteEffects.None, 0f);
            }
        }
    }

    public class DeathbringerShard : ModProjectile
    {
        private static readonly Color Blood = new Color(235, 45, 70);

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.ai[0] > 12f)
            {
                NPC best = null;
                float bestDist = 750f;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (!n.CanBeChasedBy(Projectile)) continue;

                    float d = Vector2.Distance(Projectile.Center, n.Center);
                    if (d >= bestDist) continue;

                    bestDist = d;
                    best = n;
                }

                if (best != null)
                {
                    float speed = Math.Max(Projectile.velocity.Length(), 14f);
                    Vector2 desired = (best.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * speed;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.12f);
                }
            }

            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch,
                        -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.4f, 0.4f), 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                    dust.noGravity = true;
                }

                Lighting.AddLight(Projectile.Center, new Vector3(0.6f, 0.1f, 0.18f));
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = -0.2f }, Projectile.position);

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch,
                    Main.rand.NextVector2Circular(3.5f, 3.5f), 80, default, Main.rand.NextFloat(0.9f, 1.4f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Color tone = Blood with { A = 0 };

            for (int i = Projectile.oldPos.Length - 1; i >= 1; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float k = i / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null,
                    tone * ((1f - k) * 0.6f), Projectile.oldRot[i], origin, Projectile.scale * (1.2f - k * 0.6f), SpriteEffects.None, 0);
            }

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(tex, pos, null, tone * 0.6f, Projectile.rotation, origin, Projectile.scale * 1.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, pos, null, Color.White with { A = 0 } * 0.8f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}