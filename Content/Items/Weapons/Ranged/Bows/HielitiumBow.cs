using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Ranged.Bows;

namespace Waybound.Content.Items.Weapons.Ranged.Bows
{
    public class HielitiumBow : BaseHoldoutBow
    {
        public override int ProjectileType => ModContent.ProjectileType<HielitiumBowHoldout>();
        public override int Damage => 20;
        public override int UseTime => 18;
        public override int ShotCooldown => 22;
        public override float HoldoutDistance => 8f;
        public override float BaseOffset => 8f;
        public override int AmmoType => AmmoID.Arrow;
        public override int Rarity => ItemRarityID.Pink;
        public override int Value => Item.sellPrice(gold: 4);
        public override float KnockBack => 3.5f;
        public override SoundStyle UseSound => SoundID.Item5;
        public override SoundStyle ShotSound => SoundID.Item5;
        public override int DustType => DustID.IceTorch;
        public override Vector2? HoldoutOffset() => new Vector2(-6f, 0f);
    }
    public class HielitiumBowHoldout : BaseHoldoutProjectile
    {
        public override int ShotCooldown => 45;
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Bows/HielitiumBow";
        public override float HoldoutDistance => 10f;
        public override float BaseOffset => 8f;
        public override int DustType => DustID.IceTorch;
        public override int ProjectileType => ModContent.ProjectileType<HielitiumBowP>();
        public override SoundStyle ShotSound => SoundID.Item5;
        public override float ProjectileSpeed => 11f;
        public override float SpawnOffset => 38f;
        public override int DustCount => 10;
        public override float DustScale => 1.3f;
        public override Vector3 LightColor => new Vector3(0.25f, 0.55f, 0.85f);

        private int shotCount;

        protected override void FireShot(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return;

            Item heldItem = player.HeldItem;
            if (!player.HasAmmo(heldItem))
                return;

            Vector2 spawnPos = Projectile.Center + Projectile.velocity * SpawnOffset;
            if (Collision.SolidCollision(spawnPos, 4, 4))
                spawnPos = player.Center + Projectile.velocity * (SpawnOffset + 5f);

            Vector2 velocity = Projectile.velocity * ProjectileSpeed;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;

            float charge = 1f;
            float speedMult = ChargeSpeedMultMin + charge * (ChargeSpeedMultMax - ChargeSpeedMultMin);
            float damageMult = ChargeDamageMultMin + charge * (ChargeDamageMultMax - ChargeDamageMultMin);

            int dmg = (int)(damage * damageMult);
            float kb = knockback * charge;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                velocity * speedMult,
                ProjectileType,
                dmg,
                kb,
                player.whoAmI
            );

            shotCount++;
            if (shotCount % 2 == 0)
            {
                float angle = MathHelper.ToRadians(20f);
                Vector2 velUp = velocity.RotatedBy(-angle) * speedMult;
                Vector2 velDown = velocity.RotatedBy(angle) * speedMult;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velUp,
                    ProjectileType,
                    dmg,
                    kb,
                    player.whoAmI
                );
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    velDown,
                    ProjectileType,
                    dmg,
                    kb,
                    player.whoAmI
                );
            }

            _recoilProgress = RecoilAmount;
            _targetHoldoutDistance = 2f;

            SoundEngine.PlaySound(ShotSound with { Pitch = 0.1f + charge * 0.3f }, Projectile.Center);

            for (int i = 0; i < DustCount; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    spawnPos - new Vector2(4, 4),
                    8, 8,
                    DustType,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-0.5f, 0.5f),
                    0,
                    default,
                    DustScale
                );
                dust.noGravity = true;
                dust.alpha = 80;
            }

            for (int i = 0; i < SmokeDustCount; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    spawnPos - new Vector2(4, 4),
                    8, 8,
                    SmokeDustType,
                    Main.rand.NextFloat(-0.5f, 0f),
                    Main.rand.NextFloat(-0.2f, 0.2f),
                    0,
                    default,
                    SmokeDustScale
                );
                dust.noGravity = true;
                dust.alpha = 200;
            }
        }
    }

}