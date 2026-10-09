using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Weapons.Melee.Pans;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class MoltenRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<MoltenRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1);
            Item.damage = 32;
            Item.knockBack = 5.5f;
            Item.crit = 8;
        }
    }

    public class MoltenRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/MoltenRapier";

        public override float EmpoweredDistance => 235f;
        protected override float GetThrustDistance(int i) => i switch { 0 => 115f, 1 => 130f, _ => 152f };
        protected override int WindupFrames(int i) => i == 2 ? 20 : 4;
        protected override int RecoverFrames(int i) => i == 2 ? 16 : 6;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<MoltenRapier>();

        protected override Color GetLungeParticleColor() => new Color(255, 120, 40, 255);
        protected override Color GetTipParticleColor() => new Color(255, 90, 20, 255);
        protected override Color GetTrailParticleColor() => new Color(255, 100, 30, 200);
        protected override Color GetGlowColor() => new Color(255, 160, 60, 200);
        protected override Color GetGreenPulseColor() => new Color(255, 180, 50, 220);
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.AddBuff(BuffID.OnFire, empowered ? 360 : 150);
        }

        protected override void OnEmpoweredStrike(Player player)
        {
            int damage = (int)(Projectile.damage * 0.8f);
            int projType = ModContent.ProjectileType<IncandescencedPanP>();

            for (int i = -1; i <= 1; i++)
            {
                float angle = i * 0.13f + Main.rand.NextFloat(-0.03f, 0.03f);
                Vector2 vel = Projectile.velocity.RotatedBy(angle) * 13f;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Tip,
                    vel,
                    projType,
                    damage,
                    Projectile.knockBack,
                    player.whoAmI);
            }

            for (int i = 0; i < 18; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4.5f, 4.5f) + Projectile.velocity * Main.rand.NextFloat(2f, 6f);
                ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                    Tip.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(18f, 30f)),
                    new Color(255, 140, 40, 220),
                    Main.rand.Next(24, 40)));
            }
        }
    }
}