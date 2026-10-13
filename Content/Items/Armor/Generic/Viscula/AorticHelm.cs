using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Waybound.Common.ModSystems;
using Waybound.Common.Rarities;
using Waybound.Common.WUtils;
using Waybound.Content.Dusts;

namespace Waybound.Content.Items.Armor.Generic.Viscula
{
    [AutoloadEquip(EquipType.Head)]
    public class AorticHelm : ModItem
    {
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = Item.sellPrice(0, 3, 20, 0);
            Item.rare = ItemRarityID.Purple;
            Item.defense = 10;
        }
        public override void UpdateEquip(Player player)
        {
            player.GetCritChance(DamageClass.Throwing) += 0.18f;
            player.moveSpeed += 0.08f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ItemType<AorticBreastplate>() && legs.type == ItemType<AorticBreeches>() && head.type == Type;
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = Loc.GetTips("Armor.AorticSetBonus");
            player.GetModPlayer<AorticPlayer>().bloodkeeper = true;
        }
    }

    public class AorticPlayer : ModPlayer
    {
        public const int CooldownTime = 60 * 60 * 3;
        public const float SurviveLifeFraction = 0.2f;
        public const int SurviveImmunity = 120;

        public bool bloodkeeper;
        public int cooldown;

        public override void ResetEffects() => bloodkeeper = false;

        public override void PostUpdateEquips()
        {
            if (!bloodkeeper || Player.whoAmI != Main.myPlayer)
                return;

            int type = ModContent.ProjectileType<Bloodkeeper>();
            if (Player.ownedProjectileCounts[type] < 1)
                Projectile.NewProjectile(Player.GetSource_Misc("AorticSet"), Player.Center, Vector2.Zero, type, 0, 0f, Player.whoAmI);
        }

        public override void PostUpdate()
        {
            if (cooldown > 0)
                cooldown--;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (!bloodkeeper || cooldown > 0)
                return true;

            cooldown = CooldownTime;

            Player.statLife = Math.Max(1, (int)(Player.statLifeMax2 * SurviveLifeFraction));
            Player.immune = true;
            Player.SetImmuneTimeForAllTypes(SurviveImmunity);

            for (int i = 0; i < 26; i++)
            {
                Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(Player.width / 2f, Player.height / 2f),
                    DustID.Blood, Main.rand.NextVector2Circular(5f, 5f), 100, default, Main.rand.NextFloat(1.2f, 1.9f));
                d.noGravity = Main.rand.NextBool(2);
            }

            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.7f, Pitch = -0.5f }, Player.Center);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, Player.whoAmI);

            return false;
        }

        public override void SaveData(TagCompound tag)
        {
            if (cooldown > 0)
                tag["aorticCooldown"] = cooldown;
        }

        public override void LoadData(TagCompound tag)
        {
            cooldown = tag.GetInt("aorticCooldown");
        }
    }

    public class Bloodkeeper : ModProjectile
    {
        private const int BeatPeriod = 58;

        private int beatTimer;
        private float beat;
        private float flash;
        private float deadMix;
        private bool initialized;
        private bool wasLifeless;

        private bool Lifeless => Projectile.ai[1] > 0.5f;

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.netImportant = true;
            Projectile.spriteDirection = -1;
        }

        public override bool? CanDamage() => false;

        public override bool? CanCutTiles() => false;

        private static float Bump(float t, float start, float length)
        {
            float x = (t - start) / length;
            if (x <= 0f || x >= 1f)
                return 0f;

            float s = MathF.Sin(x * MathHelper.Pi);
            return s * s;
        }

        private static float Pulse(int t) => MathF.Max(Bump(t, 0f, 15f), Bump(t, 19f, 13f) * 0.6f);

        private void Burst(int type, int count, float speed, float minScale, float maxScale, bool gravity)
        {
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, type,
                    Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(0.4f, 1f) * speed,
                    100, default, Main.rand.NextFloat(minScale, maxScale));
                d.noGravity = !gravity || Main.rand.NextBool(2);
            }
        }

        private void Thump(bool strong)
        {
            SoundEngine.PlaySound(SoundID.NPCHit1 with { Volume = strong ? 0.16f : 0.09f, Pitch = strong ? -0.85f : -0.6f }, Projectile.Center);

            if (!strong)
                return;

            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.Blood,
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.8f, 1.8f), 120, default, Main.rand.NextFloat(0.8f, 1.1f));
                d.noGravity = true;
            }
        }

        private void OnLifeless()
        {
            beat = 0f;
            flash = 0f;
            Burst(DustID.Blood, 24, 5f, 1.1f, 1.7f, true);
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.5f, Pitch = -0.7f }, Projectile.Center);
        }

        private void OnRevive()
        {
            beatTimer = 0;
            flash = 1f;
            Burst(DustID.RedTorch, 30, 6f, 1.2f, 1.9f, false);
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.7f, Pitch = -0.2f }, Projectile.Center);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            AorticPlayer modPlayer = player.GetModPlayer<AorticPlayer>();

            if (!player.active || player.dead || !modPlayer.bloodkeeper)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            if (Projectile.owner == Main.myPlayer)
            {
                float state = modPlayer.cooldown > 0 ? 1f : 0f;
                if (Projectile.ai[1] != state)
                {
                    Projectile.ai[1] = state;
                    Projectile.netUpdate = true;
                }
            }

            float time = (float)Main.GameUpdateCount;

            if (!initialized)
            {
                initialized = true;
                wasLifeless = Lifeless;
                deadMix = Lifeless ? 1f : 0f;
                Projectile.Center = player.Center + new Vector2(-player.direction * 38f, -34f);
            }
            else if (Lifeless != wasLifeless)
            {
                wasLifeless = Lifeless;
                if (Lifeless)
                    OnLifeless();
                else
                    OnRevive();
            }

            deadMix = MathHelper.Clamp(deadMix + (Lifeless ? 0.03f : -0.05f), 0f, 1f);
            flash = Math.Max(0f, flash - 0.03f);

            Vector2 goal;
            float follow;

            if (!Lifeless)
            {
                goal = player.Center + new Vector2(-player.direction * 38f + MathF.Cos(time * 0.037f) * 6f, -34f + MathF.Sin(time * 0.05f) * 6f);
                follow = 0.12f;
            }
            else
            {
                goal = player.Center + new Vector2(-player.direction * 48f + MathF.Cos(time * 0.021f) * 12f, -6f + MathF.Sin(time * 0.029f) * 8f);
                follow = 0.045f;
            }

            Vector2 diff = goal - Projectile.Center;
            Vector2 desired = diff.SafeNormalize(Vector2.Zero) * Math.Min(diff.Length() * 0.16f, 18f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, follow);

            if (diff.LengthSquared() > 1600f * 1600f)
            {
                Projectile.Center = goal;
                Projectile.velocity = Vector2.Zero;
            }

            if (MathF.Abs(Projectile.velocity.X) > 0.35f)
                Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;

            if (!Lifeless)
            {
                beatTimer++;
                if (beatTimer >= BeatPeriod)
                    beatTimer = 0;

                if (beatTimer == 0)
                    Thump(true);
                else if (beatTimer == 19)
                    Thump(false);

                beat = Pulse(beatTimer);
                Projectile.rotation = Projectile.velocity.X * 0.04f + MathF.Sin(time * 0.04f) * 0.07f;

                float glow = 0.25f + 0.75f * MathF.Max(beat, flash);
                Lighting.AddLight(Projectile.Center, 0.75f * glow, 0.05f * glow, 0.12f * glow);
            }
            else
            {
                beat = 0f;
                Projectile.rotation += 0.014f * (Projectile.identity % 2 == 0 ? 1f : -1f) + Projectile.velocity.X * 0.002f;

                if (Main.rand.NextBool(22))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.Blood,
                        new Vector2(0f, Main.rand.NextFloat(0.3f, 1f)), 150, new Color(70, 20, 25), Main.rand.NextFloat(0.7f, 1f));
                    d.noGravity = false;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            SpriteEffects fx = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float time = Main.GlobalTimeWrappedHourly;

            float alive = 1f - deadMix;
            float pulse = MathF.Max(beat, flash);
            float scale = Projectile.scale * (1f + 0.2f * pulse) * MathHelper.Lerp(1f, 0.92f, deadMix);

            if (alive > 0.02f)
            {
                float g = alive * (0.3f + 0.7f * pulse);

                Main.EntitySpriteDraw(tex, pos, null, new Color(190, 15, 40, 0) * (g * 0.55f),
                    Projectile.rotation, origin, scale * (1.35f + 0.5f * pulse), fx, 0);

                Color mid = new Color(255, 60, 70, 0) * (g * 0.4f);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = (MathHelper.PiOver2 * i + time * 1.2f).ToRotationVector2() * (2f + 4f * pulse);
                    Main.EntitySpriteDraw(tex, pos + offset, null, mid, Projectile.rotation, origin, scale * (1.05f + 0.15f * pulse), fx, 0);
                }
            }

            Color lively = Color.Lerp(lightColor, Color.White, 0.3f);
            lively = Color.Lerp(lively, new Color(255, 110, 120), pulse * 0.4f);
            Color dead = Color.Lerp(lightColor, new Color(35, 28, 34), 0.65f) * 0.88f;

            Main.EntitySpriteDraw(tex, pos, null, Color.Lerp(lively, dead, deadMix), Projectile.rotation, origin, scale, fx, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            Burst(DustID.Blood, 8, 3f, 0.9f, 1.3f, true);
        }
    }
}