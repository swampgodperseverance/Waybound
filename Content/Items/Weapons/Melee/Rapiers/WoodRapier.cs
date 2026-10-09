using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class WoodRapier : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<WoodRapierHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 20);
            Item.damage = 4;
            Item.knockBack = 4f;
            Item.crit = 6;
        }
    }

    public class WoodRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/WoodRapier";
        public override bool UseSpecial => false;
        public override float BaseHoldoutDistance => 24f;
        public override float BladeLength => 46f;

        protected override float GetThrustDistance(int i) => i switch { 0 => 70f, 1 => 80f, _ => 95f };
        protected override int WindupFrames(int i) => 3;
        protected override int ThrustFrames(int i) => 2;
        protected override int HoldFrames(int i) => 1;
        protected override int RecoverFrames(int i) => 5;
        protected override int EndFrames => 6;

        protected override float GetDamageMultiplier() => thrustIndex == 2 ? 1.1f : 0.8f;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<WoodRapier>();

        protected override Color GetLungeParticleColor() => new Color(160, 105, 55, 255);
        protected override Color GetTipParticleColor() => new Color(145, 95, 45, 255);
        protected override Color GetTrailParticleColor() => new Color(150, 100, 50, 200);
        protected override Color GetGlowColor() => new Color(180, 130, 70, 180);
    }
}