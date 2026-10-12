using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Ranged.Guns.PreHM
{
    public class SVD : WarGun
    {
        public override int MagSize => 10;
        public override int ModeCount => 1;
        public override bool ModesOnRightClick => false;
        public override ModeInfo Mode(int mode) => new ModeInfo(26, 26, 0, false, 1f, 2);

        public override int SweepTime => 62;
        public override int JamTime => 120;
        public override float GoodHalf => 0.09f;
        public override float PerfectHalf => 0.028f;
        public override int FinishPerfect => 10;
        public override int FinishGood => 20;

        public override float HoldDistance => 26f;
        public override float MuzzleDistance => 62f;
        public override float KickAngle => 0.28f;
        public override int KickTime => 12;
        public override Color Tracer => new Color(255, 235, 150);
        public override Color MeterColor => new Color(120, 255, 170);
        public override string ExtraTip => "";
        public override SoundStyle ShotSound => SoundID.Item40 with { Volume = 0.8f, Pitch = -0.1f, PitchVariance = 0.05f };

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 74;
            Item.height = 20;
            Item.damage = 42;
            Item.crit = 10;
            Item.knockBack = 4.5f;
            Item.shootSpeed = 14f;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 12);
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);
            player.scope = true;
        }

        public override void UpdateHeld(Player player, GunState s)
        {
            bool still = Math.Abs(player.velocity.X) < 0.3f && Math.Abs(player.velocity.Y) < 0.3f;
            s.Meter = still ? Math.Min(1f, s.Meter + 1f / 45f) : Math.Max(0f, s.Meter - 0.05f);
        }

        public override float ComputeSpread(Player player, GunState s) => 0.035f * (1f - s.Meter);

        public override float DamageMult(Player player, GunState s) => 1f + 0.4f * s.Meter;

        public override void OnShot(Player player, GunState s) => s.Meter = 0f;

        public override float MeterValue(GunState s) => s.Meter;

        public override void ModifyBulletHit(NPC target, ref NPC.HitModifiers modifiers, bool boosted)
        {
            if (boosted)
                modifiers.ScalingArmorPenetration += 0.5f;
        }

        public override void AddRecipes()
        {

        }
    }

    public class M249 : WarGun
    {
        public override int MagSize => 100;
        public override int ModeCount => 2;

        public override ModeInfo Mode(int mode)
        {
            return mode == 0
                ? new ModeInfo(10, 10, 0, true, 0.9f, 1)
                : new ModeInfo(10, 10, 0, true, 1.2f, 2);
        }

        public override int SweepTime => 84;
        public override int JamTime => 160;
        public override float GoodHalf => 0.13f;
        public override float PerfectHalf => 0.04f;
        public override int FinishPerfect => 12;
        public override int FinishGood => 30;

        public override float HoldDistance => 24f;
        public override float MuzzleDistance => 54f;
        public override float KickAngle => 0.12f;
        public override int KickTime => 6;
        public override float AmmoSave => 0.5f;
        public override Color Tracer => new Color(255, 150, 60);
        public override Color MeterColor => new Color(255, 150, 60);
        public override int[] ModeBars => new[] { 6, 2 };
        public override Color[] ModeColors => new[] { new Color(255, 130, 80), new Color(120, 200, 255) };
        public override string ExtraTip => "";
        public override SoundStyle ShotSound => SoundID.Item11 with { Volume = 0.7f, Pitch = -0.2f, PitchVariance = 0.1f };

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 70;
            Item.height = 26;
            Item.damage = 11;
            Item.knockBack = 2f;
            Item.shootSpeed = 9f;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 10);
        }

        public override int UseTimeFor(GunState s, ModeInfo m) => Math.Max(5, (int)MathHelper.Lerp(10f, 5f, s.Meter));

        public override float ComputeSpread(Player player, GunState s) => s.FireMode == 0 ? s.Spread : 0.012f;

        public override void OnShot(Player player, GunState s)
        {
            s.Meter = Math.Min(1f, s.Meter + 0.035f);
            if (s.FireMode == 0)
                s.Spread = Math.Min(s.Spread + 0.012f, 0.16f);
        }

        public override void UpdateHeld(Player player, GunState s)
        {
            if (s.SpreadCooldown <= 0)
                s.Meter = Math.Max(0f, s.Meter - 0.02f);
        }

        public override void ModifyMovement(Player player, GunState s)
        {
            if (s.FireMode != 1)
                return;

            player.maxRunSpeed *= 0.35f;
            player.accRunSpeed = Math.Min(player.accRunSpeed, player.maxRunSpeed);
            player.runAcceleration *= 0.5f;
        }

        public override float MeterValue(GunState s) => s.Meter;

        public override void AddRecipes()
        {

        }
    }

    public class MauserC96 : WarGun
    {
        public override int MagSize => 10;
        public override int ModeCount => 2;

        public override ModeInfo Mode(int mode)
        {
            return mode == 0
                ? new ModeInfo(14, 14, 0, false, 1.25f, 2)
                : new ModeInfo(5, 5, 0, true, 0.75f, 1);
        }

        public override int SweepTime => 40;
        public override int JamTime => 70;
        public override float GoodHalf => 0.12f;
        public override float PerfectHalf => 0.05f;
        public override int FinishPerfect => 5;
        public override int FinishGood => 10;

        public override bool OneHanded => true;
        public override float HoldDistance => 14f;
        public override float MuzzleDistance => 30f;
        public override float KickAngle => 0.3f;
        public override int KickTime => 7;
        public override float MagDrop => 14f;
        public override Color Tracer => new Color(255, 210, 120);
        public override int[] ModeBars => new[] { 1, 6 };
        public override Color[] ModeColors => new[] { new Color(255, 200, 90), new Color(255, 110, 90) };
        public override string ExtraTip => "";
        public override SoundStyle ShotSound => SoundID.Item41 with { Volume = 0.7f, PitchVariance = 0.08f };

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 32;
            Item.height = 22;
            Item.damage = 16;
            Item.knockBack = 2f;
            Item.shootSpeed = 8.5f;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 2);
        }

        public override float ComputeSpread(Player player, GunState s) => s.FireMode == 0 ? 0.02f : s.Spread;

        public override void OnShot(Player player, GunState s)
        {
            if (s.FireMode == 1)
                s.Spread = Math.Min(s.Spread + 0.014f, 0.15f);
        }

        public override void AddRecipes()
        {

        }
    }   
}