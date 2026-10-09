using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.ID;
using Waybound.Content.Projectiles.Base;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Heavy;

public class Tidebreaker : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 68;
        Item.height = 72;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.holdStyle = 0;
        Item.noUseGraphic = true;
        Item.UseSound = SoundID.Item7;
        Item.autoReuse = true;
        Item.DamageType = DamageClass.Melee;
        Item.damage = 800;
        Item.knockBack = 3f;
        Item.value = Item.sellPrice(0, 10);
        Item.rare = ItemRarityID.Cyan;
        Item.noMelee = true;
        Item.useTurn = false;
        Item.shoot = ProjectileType<TidebreakerProjectile>();
        Item.shootSpeed = 0f;
    }
}

public class TidebreakerProjectile : BaseHeavySword
{
    public override string Texture => "Waybound/Content/Items/Weapons/Melee/Heavy/Tidebreaker";

    protected override int MaxCombo => 10;

    protected override bool CanUseSpecialAttack() => Combo > 0;

    protected override float SwordAngleOffset => 45f;

    protected override float SwordHiltOffset => 2f;

    protected override float HitboxSize => 12f;

    protected override int BaseSwingTime => 30;

    protected override int ComboContinueTime => 20;

    protected override float SwordLength => 90f;
    
    protected override void SetDefaults_Extra()
    {
        Projectile.scale = 1.25f;
    }
    
    // protected override void SpawnParticles()
    // {
    //     Vector2 swordEdgePosition = GetSwordEdgePosition();
    //     
    //     ParticleSystem.TrailBuffer.Create(new ParticleInfo(
    //         position: Vector2.Lerp(Projectile.Center, swordEdgePosition, 0.9f).ToNumerics(),
    //         velocity: System.Numerics.Vector2.Zero,
    //         rotation: Projectile.rotation * -Projectile.spriteDirection,
    //         scale: new System.Numerics.Vector2(96f, 48f),
    //         color: new Color(172, 227, 255, 80),
    //         duration: 45
    //     ));
    //
    //     for (int i = 0; i < 5; i++)
    //     {
    //         float progress =  (float)i / 5;
    //         Dust.NewDust(
    //             Vector2.Lerp(Projectile.Center, swordEdgePosition, progress),
    //             16, 16,
    //             DustID.Water_Snow
    //         );
    //     }
    // }

    public override void PostAI()
    {
        Lighting.AddLight(
            Vector2.Lerp(Projectile.Center, GetSwordEdgePosition(), 0.5f),
            new Vector3(172, 227, 255) / 255f * 1.5f
        );
    }
}