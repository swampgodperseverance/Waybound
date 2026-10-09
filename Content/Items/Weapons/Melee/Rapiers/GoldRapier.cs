using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
 
    public class RapierBlinkPlayer : ModPlayer
    {
        public int BlinkCooldown;

        public override void UpdateDead() => BlinkCooldown = 0;

        public override void PostUpdate()
        {
            if (BlinkCooldown <= 0)
                return;

            BlinkCooldown--;

            if (BlinkCooldown == 0 && Player.whoAmI == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.3f, Volume = 0.6f }, Player.Center);
                for (int i = 0; i < 12; i++)
                {
                    Dust d = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(18f, 22f),
                        DustID.MagicMirror, Main.rand.NextVector2Circular(1.5f, 1.5f), 100, default, 1.2f);
                    d.noGravity = true;
                }
            }
        }
    }

   /// <summary>
   /// rapier class in the rapier class inside fucking rapier classes am I a fucking genius?
   /// </summary>
    public abstract class BaseBlinkRapierHoldout : BaseRapierHoldout
    {
        protected abstract float BlinkDistance { get; }
        protected abstract int BlinkCooldownFrames { get; }
        protected abstract int BlinkDust { get; }
        protected virtual int BlinkImmuneFrames => 10;
        protected virtual float MinBlinkDistance => 48f;

        private bool blinked;

        public override float EmpoweredDistance => BlinkDistance;
        public override bool UseSpecial =>
            Main.player[Projectile.owner].GetModPlayer<RapierBlinkPlayer>().BlinkCooldown <= 0;

        protected override void OnEmpoweredStrike(Player player)
        {
            Vector2 start = player.Center;
            Vector2 dir = Projectile.velocity;

            float reach = MathHelper.Clamp(Vector2.Distance(start, Main.MouseWorld), MinBlinkDistance, BlinkDistance);
            Vector2 end = FindBlinkEnd(player, start, dir, reach);
            float dist = Vector2.Distance(start, end);

            if (dist < MinBlinkDistance)
            { 
                SoundEngine.PlaySound(SoundID.Item16 with { Pitch = -0.3f }, start);
                for (int i = 0; i < 10; i++)
                    Spark(Tip + Main.rand.NextVector2Circular(10f, 10f), Main.rand.NextVector2Circular(2f, 2f),
                        new Color(110, 110, 120, 200), Main.rand.NextFloat(6f, 10f), 14);
                return;
            }

            for (float d = 0f; d < dist; d += 10f)
            {
                Vector2 p = start + dir * d;
                Spark(p + Main.rand.NextVector2Circular(6f, 6f), Vector2.Zero, GetGreenPulseColor(),
                    Main.rand.NextFloat(10f, 18f), Main.rand.Next(18, 30));

                if (Main.rand.NextBool(2))
                {
                    Dust t = Dust.NewDustPerfect(p, BlinkDust, dir * Main.rand.NextFloat(0.5f, 2f), 100, default, 1.3f);
                    t.noGravity = true;
                }
            }

            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f, Volume = 0.9f }, start);

            player.Teleport(end - player.Size / 2f, 1);
            player.velocity = dir * 5f;
            player.fallStart = (int)(player.position.Y / 16f);
            player.SetImmuneTimeForAllTypes(BlinkImmuneFrames);

            for (int i = 0; i < 24; i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 6f);
                Dust a = Dust.NewDustPerfect(end, BlinkDust, v, 100, default, 1.5f);
                a.noGravity = true;
            }
            for (int i = 0; i < 14; i++)
                Spark(end, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 7f),
                    GetGreenPulseColor(), Main.rand.NextFloat(8f, 14f), Main.rand.Next(16, 28));

            blinked = true;
        }

        private static Vector2 FindBlinkEnd(Player player, Vector2 start, Vector2 dir, float maxDist)
        {
            Vector2 last = start;
            float minX = 16f * 12f;
            float maxX = (Main.maxTilesX - 12) * 16f;
            float minY = 16f * 12f;
            float maxY = (Main.maxTilesY - 12) * 16f;

            for (float d = 8f; d <= maxDist; d += 8f)
            {
                Vector2 p = start + dir * d;
                if (p.X < minX || p.X > maxX || p.Y < minY || p.Y > maxY)
                    break;

                Vector2 topLeft = p - player.Size / 2f;
                if (Collision.SolidCollision(topLeft, player.width, player.height))
                    break;
                if (Collision.LavaCollision(topLeft, player.width, player.height))
                    break;

                last = p;
            }
            return last;
        }

        public override void OnKill(int timeLeft)
        {
            if (blinked && Projectile.owner == Main.myPlayer)
                Main.player[Projectile.owner].GetModPlayer<RapierBlinkPlayer>().BlinkCooldown = BlinkCooldownFrames;
        }
    }

    public class GoldRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<GoldRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 45;
            Item.height = 45;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 18);
            Item.damage = 15;
            Item.knockBack = 4.8f;
            Item.crit = 6;
        }
    }

    public class GoldRapierHoldout : BaseBlinkRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/GoldRapier";

        protected override float BlinkDistance => 210f;
        protected override int BlinkCooldownFrames => 420; 
        protected override int BlinkImmuneFrames => 10;
        protected override int BlinkDust => DustID.Gold;

        protected override float GetThrustDistance(int i) => i switch { 0 => 108f, 1 => 122f, _ => 142f };
        protected override int WindupFrames(int i) => i == 2 ? 22 : 4;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<GoldRapier>();

        protected override Color GetLungeParticleColor() => new Color(230, 190, 60, 255);
        protected override Color GetTipParticleColor() => new Color(215, 175, 45, 255);
        protected override Color GetTrailParticleColor() => new Color(220, 180, 50, 200);
        protected override Color GetGlowColor() => new Color(250, 210, 80, 190);
        protected override Color GetGreenPulseColor() => new Color(255, 230, 80, 220);
    }

    public class PlatinumRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<PlatinumRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 27);
            Item.damage = 16;
            Item.knockBack = 5f;
            Item.crit = 6;
        }
    }

    public class PlatinumRapierHoldout : BaseBlinkRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/PlatinumRapier";

        protected override float BlinkDistance => 260f;
        protected override int BlinkCooldownFrames => 360; 
        protected override int BlinkImmuneFrames => 14;
        protected override int BlinkDust => DustID.Platinum;

        protected override float GetThrustDistance(int i) => i switch { 0 => 112f, 1 => 128f, _ => 150f };
        protected override int WindupFrames(int i) => i == 2 ? 22 : 4;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<PlatinumRapier>();

        protected override Color GetLungeParticleColor() => new Color(210, 220, 235, 255);
        protected override Color GetTipParticleColor() => new Color(195, 205, 220, 255);
        protected override Color GetTrailParticleColor() => new Color(200, 210, 225, 200);
        protected override Color GetGlowColor() => new Color(230, 240, 250, 190);
        protected override Color GetGreenPulseColor() => new Color(180, 220, 255, 220);
    }
}