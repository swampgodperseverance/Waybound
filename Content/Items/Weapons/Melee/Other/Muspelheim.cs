using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Waybound.Common.ModSystems;
using Waybound.Content.Items.Weapons.Melee.Other;
using Waybound.Helpers;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Other
{
    public class Muspelheim : ModItem
    {
        private const float UltDamageMultiplier = 2.2f;
        private const float HookDamageMultiplier = 2.2f;

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Phase == WeaponPhase.Spear)
                return true; 

            Texture2D tex = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Melee/Other/MuspelheimChain").Value;
            spriteBatch.Draw(tex, position, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (Phase == WeaponPhase.Spear)
                return true;

            Texture2D tex = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Melee/Other/MuspelheimChain").Value;
            Vector2 origin = tex.Size() * 0.5f;
            spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            var mp = player.GetModPlayer<MuspelheimPlayer>();
            if (mp.UltActive)
                return false;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimSpear>()] > 0)
                return false;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimDive>()] > 0)
                return false;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimHook>()] > 0)
                return false;

            if (player.altFunctionUse == 2)
            {
                if (Phase == WeaponPhase.Chain)
                    return mp.  HookCooldown <= 0
                        && player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimChainProj>()] == 0;

                return mp.Full && MuspelheimDive.TryGetDestination(player, out _);
            }
            return true;
        }
        public enum WeaponPhase { Spear, Chain }

        public WeaponPhase Phase = WeaponPhase.Spear;
        public override void SetDefaults()
        {
            ApplyPhase(Phase);
        }

        private void ApplyPhase(WeaponPhase phase)
        {
            Phase = phase;

            if (phase == WeaponPhase.Spear)
            {
                Item.damage = 210;
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.knockBack = 5.5f;
                Item.channel = false;
                Item.autoReuse = true;
                Item.shoot = ModContent.ProjectileType<MuspelheimSpear>();
                Item.shootSpeed = 1f;
            }
            else
            {
                Item.damage = 250;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.knockBack = 4.5f;
                Item.channel = true;
                Item.autoReuse = false;
                Item.shoot = ModContent.ProjectileType<MuspelheimChainProj>();
                Item.shootSpeed = 1f;
            }

            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.value = Item.sellPrice(0, 4, 50);
            Item.rare = ItemRarityID.Orange;
            Item.noUseGraphic = true;
            Item.noMelee = true;
        }

        public void SwitchPhase()
        {
            ApplyPhase(Phase == WeaponPhase.Spear ? WeaponPhase.Chain : WeaponPhase.Spear);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.SafeNormalize(Vector2.UnitX);
            if (player.altFunctionUse == 2)
            {
                if (Phase == WeaponPhase.Chain)
                {
                    type = ModContent.ProjectileType<MuspelheimHook>();
                    damage = (int)(damage * HookDamageMultiplier);
                }
                else
                {
                    type = ModContent.ProjectileType<MuspelheimDive>();
                    damage = (int)(damage * UltDamageMultiplier);
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                if (Phase == WeaponPhase.Chain)
                {
                    Projectile.NewProjectile(source, player.MountedCenter, velocity * MuspelheimHook.FlySpeed, type, damage, knockback, player.whoAmI);
                    return false;
                }

                var mp = player.GetModPlayer<MuspelheimPlayer>();
                if (MuspelheimDive.TryGetDestination(player, out Vector2 dest))
                {
                    mp.Charge = 0f;
                    mp.UltActive = true;
                    Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero,
                        ModContent.ProjectileType<MuspelheimDive>(), damage, knockback,
                        player.whoAmI, 0f, dest.X, dest.Y);
                }
                return false;
            }

            if (Phase == WeaponPhase.Spear)
            {
                Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
                return false;
            }

            Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, type, damage, knockback,
                player.whoAmI, 0f, player.direction);
            return false;
        }
    }

    public class MuspelheimPlayer : ModPlayer
    {
        public const float MaxCharge = 100f;
        public const float DiveSpeed = 30f;
        public int HookCooldown;
        public bool HookPull;
        public Vector2 HookPullVelocity;
        public int Combo;
        public int ComboTimer;
        public float Charge;
        public bool Full => Charge >= MaxCharge;
        public bool UltActive;
        public bool UltFreeze;
        public bool UltDive;

        public bool HoldingSpear => Player.HeldItem.type == ModContent.ItemType<Muspelheim>();

        public static Vector2 GaugeCenter(Player p)
        {
            return p.Top + new Vector2(p.width * 0.5f, p.gfxOffY - 28f);
        }

        public void AddCharge(float amount)
        {
            bool wasFull = Full;
            Charge = Math.Min(MaxCharge, Charge + amount);
            if (!wasFull && Full && Player.whoAmI == Main.myPlayer)
            {
                MuspelheimFx.Burst(GaugeCenter(Player), 16, 3.5f, 14f, 24f);
                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f, Volume = 0.7f }, Player.Center);
            }
        }

        public void DrawChargeBar()
        {
            if (!HoldingSpear || Charge <= 0.01f || UltActive || Player.dead)
                return;

            Vector2 pos = GaugeCenter(Player) + new Vector2(-11f, 0f);

            Color color = Full
                ? Color.Lerp(new Color(255, 120, 25), new Color(255, 215, 90),
                    0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 7f))
                : Color.Lerp(new Color(150, 30, 10), new Color(255, 130, 35), Charge / MaxCharge);

            Color outline = Color.Black * 0.5f;
            float o = 1.1f; 
            DrawHelper.DrawBar(pos + new Vector2(-o, 0), (int)Charge, (int)MaxCharge, outline, 1f, scale: 1.2f);
            DrawHelper.DrawBar(pos + new Vector2(o, 0), (int)Charge, (int)MaxCharge, outline, 1f, scale: 1.2f);
            DrawHelper.DrawBar(pos + new Vector2(0, -o), (int)Charge, (int)MaxCharge, outline, 1f, scale: 1.2f);
            DrawHelper.DrawBar(pos + new Vector2(0, o), (int)Charge, (int)MaxCharge, outline, 1f, scale: 1.2f);

            DrawHelper.DrawBar(pos, (int)Charge, (int)MaxCharge, color, 1f, scale: 1.2f);
        }

        public override void PostUpdate()
        {
            if (ComboTimer > 0)
                ComboTimer--;
            else
                Combo = 0;
            if (HookCooldown > 0)
                HookCooldown--;

            if (HookPull && Player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimHook>()] == 0)
                HookPull = false;
            if (Full && !UltActive && HoldingSpear && Player.whoAmI == Main.myPlayer && !Main.dedServ)
            {
                Vector2 c = GaugeCenter(Player);
                if (Main.GameUpdateCount % 3 == 0)
                {
                    Vector2 pos = c + new Vector2(Main.rand.NextFloat(-24f, 24f), -3f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.6f, 1.4f));
                    MuspelheimFx.SpawnFire(pos, vel, Main.rand.NextFloat(10f, 16f), Main.rand.Next(20, 30));
                }
                Lighting.AddLight(c, 0.9f, 0.35f, 0.1f);
            }
        }

        public override void PostUpdateRunSpeeds()
        {
            if (UltDive)
            {
                Player.maxFallSpeed = Math.Max(Player.maxFallSpeed, DiveSpeed);
                Player.velocity.X = 0f;
                Player.velocity.Y = DiveSpeed;
            }
            else if (UltFreeze)
            {
                Player.velocity = Vector2.Zero;
            }
            else if (HookPull)
            {
                Player.velocity = HookPullVelocity;
                Player.fallStart = (int)(Player.position.Y / 16f);
            }
        }

        public override void PostUpdateEquips()
        {
            if (UltActive || HookPull)
                Player.noFallDmg = true;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            Charge = 0f;
            UltActive = false;
            UltFreeze = false;
            UltDive = false;
        }
    }

    public class MuspelheimDrawSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (index == -1)
                return;

            layers.Insert(index + 1, new LegacyGameInterfaceLayer(
                "Waybound: Muspelheim Charge",
                () =>
                {
                    if (Main.gameMenu || Main.dedServ)
                        return true;

                    Player player = Main.LocalPlayer;
                    if (player?.active == true)
                    {
                        player.GetModPlayer<MuspelheimPlayer>().DrawChargeBar(); //bimba respect to u
                    }
                    return true;
                },
                InterfaceScaleType.Game 
            ));
        }
    }

    public static class MuspelheimFx
    {
        //this was my first time of doing fx as a separate project, i do recommend
        public static float SpriteLength(int type, float scale)
        {
            if (Main.dedServ)
                return 100f * scale;
            Main.instance.LoadProjectile(type);
            Texture2D tex = TextureAssets.Projectile[type].Value;
            return new Vector2(tex.Width, tex.Height).Length() * scale;
        }

        public static void SpawnFire(Vector2 pos, Vector2 vel, float size, int duration)
        {
            if (Main.dedServ || ParticleSystem.FireBuffer == null)
                return;

            ParticleSystem.FireBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size),
                new Color(255, 140, 40),
                duration
            ));
        }

        public static void Burst(Vector2 center, int count, float speed, float minSize, float maxSize)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) * speed;
                SpawnFire(center + Main.rand.NextVector2Circular(8f, 8f), vel,
                    Main.rand.NextFloat(minSize, maxSize), Main.rand.Next(24, 42));
            }

            for (int i = 0; i < count / 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f) * speed * 1.2f;
                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 100, default, Main.rand.NextFloat(1.2f, 1.8f));
                d.noGravity = true;
            }
        }
    }
    public class MuspelheimStylePlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Player.whoAmI != Main.myPlayer || Main.dedServ)
                return;

            ModKeybind key = VanillaKeybinds.ChangeWeaponStyle;
            if (key == null || !key.JustPressed)
                return;

            TrySwapPhase();
        }

        private bool Busy()
        {
            if (Player.dead || Player.CCed || Player.noItems)
                return true;
            if (Player.GetModPlayer<MuspelheimPlayer>().UltActive)
                return true;

            return Player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimSpear>()] > 0
                || Player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimDive>()] > 0
                || Player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimChainProj>()] > 0
                || Player.ownedProjectileCounts[ModContent.ProjectileType<MuspelheimHook>()] > 0;
        }

        private void TrySwapPhase()
        {
            if (!Main.mouseItem.IsAir || Player.selectedItem < 0 || Player.selectedItem >= 50)
                return;

            Item item = Player.inventory[Player.selectedItem];

            if (item.type != ModContent.ItemType<Muspelheim>())
                return;

            if (Busy())
                return;

            if (item.ModItem is not Muspelheim muspelheim)
                return;

            int prefix = item.prefix;
            int stack = item.stack;
            bool favorited = item.favorited;

            muspelheim.SwitchPhase();

            item.stack = stack;
            item.favorited = favorited;
            if (prefix > 0)
                item.Prefix(prefix);

            Player.itemAnimation = 0;
            Player.itemTime = 0;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, Player.whoAmI, Player.selectedItem, item.prefix);

            MuspelheimFx.Burst(Player.Center, 10, 3f, 14f, 22f);
            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f, Volume = 0.6f }, Player.Center);
            CombatText.NewText(Player.getRect(), new Color(255, 150, 60),
                muspelheim.Phase == Muspelheim.WeaponPhase.Spear ? "Spear" : "Chain");
        }
    }
    public class MuspelheimVfxSystem : ModSystem
    {
        public override void Unload() => MuspelheimVfx.Unload();
    }

    public static class MuspelheimVfx
    {
        static Asset<Texture2D> star;

        public static readonly Color Hot = new Color(255, 235, 170);
        public static readonly Color Orange = new Color(255, 150, 50);
        public static readonly Color Red = new Color(255, 80, 20);

        public static Texture2D Star => (star ??= ModContent.Request<Texture2D>("Waybound/Assets/Textures/Star")).Value;

        public static void Unload() => star = null;

        public static Color Ad(Color c, float k)
            => new Color((int)Math.Clamp(c.R * k, 0f, 255f), (int)Math.Clamp(c.G * k, 0f, 255f), (int)Math.Clamp(c.B * k, 0f, 255f), 0);

        public static Color Alpha(Color c, int a) => new Color(c.R, c.G, c.B, a);

        public static float Hash(float x)
        {
            float s = MathF.Sin(x * 12.9898f) * 43758.5453f;
            return s - MathF.Floor(s);
        }

        public static void Line(Vector2 a, Vector2 b, Color c, float w)
        {
            Vector2 d = b - a;
            float l = d.Length();
            if (l < 0.5f) return;
            Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, a - Main.screenPosition, new Rectangle(0, 0, 1, 1), c,
                d.ToRotation(), new Vector2(0f, 0.5f), new Vector2(l, w), SpriteEffects.None, 0);
        }

        public static void Dot(Vector2 p, Color c, float s)
        {
            Main.EntitySpriteDraw(TextureAssets.MagicPixel.Value, p - Main.screenPosition, new Rectangle(0, 0, 1, 1), c,
                0f, new Vector2(0.5f), new Vector2(s), SpriteEffects.None, 0);
        }

        public static void Bloom(Vector2 p, Color c, float r, float rot = 0f)
        {
            Texture2D s = Star;
            Main.EntitySpriteDraw(s, p - Main.screenPosition, null, c, rot, s.Size() / 2f, r * 2f / s.Width, SpriteEffects.None, 0);
        }

        public static void Streak(Vector2 a, Vector2 b, float w, Color c, float k)
        {
            const int n = 8;
            for (int i = 0; i < n; i++)
            {
                float u0 = i / (float)n;
                float u1 = (i + 1) / (float)n;
                float f = 1f - u0;
                Line(Vector2.Lerp(a, b, u0), Vector2.Lerp(a, b, u1), Ad(c, k * f), w * f + 0.5f);
            }
        }

        public static void Ellipse(Vector2 c, float rx, float ry, float w, Color col, int seg = 40, float spin = 0f, bool dashed = false)
        {
            if (rx < 1f) return;
            Vector2 prev = c + new Vector2(MathF.Cos(spin) * rx, MathF.Sin(spin) * ry);
            for (int i = 1; i <= seg; i++)
            {
                float a = spin + MathHelper.TwoPi * i / seg;
                Vector2 cur = c + new Vector2(MathF.Cos(a) * rx, MathF.Sin(a) * ry);
                if (!dashed || i % 2 == 0) Line(prev, cur, col, w);
                prev = cur;
            }
        }

        public static void Bolt(Vector2 a, Vector2 b, float w, Color c, float seed, float amp)
        {
            const int n = 16;
            Vector2 d = (b - a).SafeNormalize(Vector2.UnitY);
            Vector2 perp = new Vector2(-d.Y, d.X);
            Vector2 prev = a;
            for (int i = 1; i <= n; i++)
            {
                float u = i / (float)n;
                Vector2 p = Vector2.Lerp(a, b, u);
                if (i < n) p += perp * (Hash(seed * 17.3f + i * 3.7f) - 0.5f) * 2f * amp * MathF.Sin(u * MathHelper.Pi);
                Line(prev, p, c, w);
                prev = p;
            }
        }

        public static void Spark(Vector2 pos, Vector2 vel, float size, Color c, int life)
        {
            if (Main.dedServ || ParticleSystem.MegasparkBuffer == null) return;
            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(), vel.ToNumerics(), Main.rand.NextFloat(MathHelper.TwoPi), new SysVector2(size), c, life));
        }

        public static void Flash(Vector2 pos, float size, Color c, int life)
        {
            if (Main.dedServ || ParticleSystem.FlashBuffer == null) return;
            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                pos.ToNumerics(), SysVector2.Zero, Main.rand.NextFloat(MathHelper.TwoPi), new SysVector2(size), c, life));
        }

        public static void Boom(Vector2 pos, Vector2 vel, float sx, float sy, int life, bool rr)
        {
            if (Main.dedServ || ParticleSystem.FlameBoomBuffer == null) return;
            ParticleSystem.FlameBoomBuffer.Create(new ParticleInfo(
                pos.ToNumerics(), vel.ToNumerics(), rr ? Main.rand.NextFloat(MathHelper.TwoPi) : 0f,
                new SysVector2(sx, sy), new Color(255, 140, 40), life));
        }

        public static void FireFlash(Vector2 pos, float scale, int dur)
        {
            Color[] cols =
            {
                new Color(255, 235, 170, 255),
                new Color(255, 150, 50, 220),
                new Color(255, 80, 20, 160)
            };
            for (int i = 0; i < 3; i++)
                Flash(pos, (40f + i * 16f) * scale + Main.rand.NextFloat(-3f, 3f), cols[i], dur + i * 2);
        }

        public static void FireRing(Vector2 c, int n, float spd, float size)
        {
            if (Main.dedServ) return;
            float off = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < n; i++)
            {
                Vector2 d = (off + MathHelper.TwoPi * i / n).ToRotationVector2();
                MuspelheimFx.SpawnFire(c + d * 8f, d * spd * Main.rand.NextFloat(0.85f, 1.15f),
                    size * Main.rand.NextFloat(0.8f, 1.2f), Main.rand.Next(18, 30));
            }
        }
    }
}