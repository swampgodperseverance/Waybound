using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Abstractions;
using Waybound.Content.Projectiles.Ranged.Bows;

namespace Waybound.Content.Items.Weapons.Ranged.Bows
{
    public class Titanoboa : BaseHoldoutBow
    {
        public override int ProjectileType => ModContent.ProjectileType<TitanoboaHoldout>();
        public override int Damage => 32;
        public override int UseTime => 40;
        public override int ShotCooldown => 35;
        public override float HoldoutDistance => 8f;
        public override float BaseOffset => 12f;
        public override int AmmoType => AmmoID.Arrow;
        public override int Rarity => ItemRarityID.Blue;
        public override int Value => Item.sellPrice(0, 0, 80, 0);
        public override float KnockBack => 2f;
        public override SoundStyle UseSound => SoundID.Item5;
        public override SoundStyle ShotSound => SoundID.Item5;
        public override int DustType => DustID.GemEmerald;
    }

    public class TitanoboaHoldout : BaseHoldoutProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Bows/Titanoboa";
        public override int ShotCooldown => 35;
        public override float HoldoutDistance => 8f;
        public override float BaseOffset => 12f;
        public override int DustType => DustID.GemEmerald;
        public override int ProjectileType => ModContent.ProjectileType<AnacondaProj>();
        public override SoundStyle ShotSound => SoundID.Item5;
    }
}