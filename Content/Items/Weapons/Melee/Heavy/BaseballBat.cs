using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Waybound.Content.Projectiles.Base;
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Heavy;

public class BaseballBat : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 46;
        Item.height = 46;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.holdStyle = 0;
        Item.noUseGraphic = true;
        Item.useTime = 40;
        Item.useAnimation = 40;
        Item.UseSound = SoundID.Item7;
        Item.autoReuse = true;
        Item.DamageType = DamageClass.Melee;
        Item.damage = 50;
        Item.knockBack = 1f;
        Item.value = Item.sellPrice(0, 10);
        Item.rare = ItemRarityID.Orange;
        Item.noMelee = true;
        Item.useTurn = false;
        Item.shoot = ProjectileType<BaseballBatProjectile>();
        Item.shootSpeed = 0f;
    }
}

public class BaseballBatProjectile : BaseHeavySword
{
    public override string Texture => "Waybound/Content/Items/Weapons/Melee/Heavy/BaseballBat";

    protected override int MaxCombo => 5;

    protected override bool CanUseSpecialAttack() => Combo == 5;

    protected override bool Colliding_OnSpecial(Rectangle projHitbox, Rectangle targetHitbox)
        => SwordColliding(targetHitbox);

    protected override bool CanHit_OnSpecial(NPC target) => SpecialTimer > WIND_UP_TIME;

    protected override void OnClick()
    {
        SoundEngine.PlaySound(SoundID.Item7,  Projectile.Center);
    }

    protected override void CastSpecialAttack()
    {
        Combo = 0;
        StartRotation = Projectile.rotation;
        Projectile.ResetLocalNPCHitImmunity();
        SoundEngine.PlaySound(SoundID.Item7,  Projectile.Center);
        CurrentState = AttackState.Special;
    }

    private ref float SpecialTimer => ref Projectile.ai[1];
    private ref float StartRotation => ref Projectile.ai[2];

    private const int WIND_UP_TIME = 30;

    private const int THROW_BALL_TIME = 15;
    
    protected override void AI_OnSpecial()
    {
        SpecialTimer++;

        if (SpecialTimer <= WIND_UP_TIME)
            WindUp();
        else if (SpecialTimer <= WIND_UP_TIME + THROW_BALL_TIME)
            Throwing();
        else
            End();
        
        Owner.SetCompositeArmFront(
            true,
            Player.CompositeArmStretchAmount.Full, 
            Projectile.rotation * -Projectile.spriteDirection
        );
    }

    private void WindUp()
    {
        float progress = SpecialTimer / WIND_UP_TIME;
        UpdateAngle(StartRotation, MaxAngle, EaseFunctions.EaseOutCubic(progress));

        float secondHandProgress = SpecialTimer * 2f / WIND_UP_TIME;
        progress = secondHandProgress < 1f
            ? EaseFunctions.EaseInCubic(secondHandProgress)
            : 1f - EaseFunctions.EaseInCubic(secondHandProgress - 1f);
        Owner.SetCompositeArmBack(
            true,
            Player.CompositeArmStretchAmount.Full, 
            MathHelper.Lerp(0, MathHelper.PiOver2, progress)
                * -Projectile.spriteDirection
        );
    }

    private void Throwing()
    {
        float time = SpecialTimer - WIND_UP_TIME;
        if ((int)time == 3)
            SpawnBall();
        
        float progress = time / THROW_BALL_TIME;
        UpdateAngle(MaxAngle, MinAngle, EaseFunctions.EaseOutCubic(progress));
        
        SpawnParticles();
    }

    private void End()
    {
        SpecialTimer = 0;
        TimeLeft = ComboContinueTime;
        //this will switch to up once lmb is clicked
        Direction = SwingDirection.Down;
        CurrentState = AttackState.Default;
    }

    private readonly Vector2 _ballSpawnOffset = new Vector2(18, -40);
    
    private void SpawnBall()
    {
        Vector2 ballPosition = Projectile.Center + _ballSpawnOffset * new Vector2(Projectile.spriteDirection, 1f);
        Vector2 direction = Main.MouseWorld - ballPosition;
        direction.Normalize();

        SoundEngine.PlaySound(SoundID.Item70,  Projectile.Center);
        if(Main.myPlayer == Projectile.owner)
            Projectile.NewProjectile(
                Terraria.Entity.GetSource_None(),
                ballPosition,
                direction * 20f,
                ProjectileType<BaseballBall>(),
                Projectile.damage * 4,
                9f
            );
        
        ParticleSystem.FlashBuffer.Create(new ParticleInfo(
            position: ballPosition.ToNumerics(),
            velocity: System.Numerics.Vector2.Zero,
            rotation: Main.rand.NextFloat(MathHelper.TwoPi),
            scale: new System.Numerics.Vector2(Main.rand.NextFloat(35f, 45f)),
            color: new Color(255, 255, 255, 255),
            duration: Main.rand.Next(8, 12)
        ));
    }
    
    protected override void Draw_OnSpecial(ref Color lightColor)
    {
        if (SpecialTimer * 2 >= WIND_UP_TIME && SpecialTimer < WIND_UP_TIME + 2)
        {
            Texture2D ballTexture = TextureAssets.Projectile[ProjectileType<BaseballBall>()].Value;
            float progress = MathHelper.Clamp((SpecialTimer - WIND_UP_TIME / 2f) / WIND_UP_TIME * 2, 0f, 1f);
            Vector2 offset = new Vector2(_ballSpawnOffset.X, MathHelper.Lerp(8, _ballSpawnOffset.Y, EaseFunctions.EaseOutCubic(progress)))
                             * new Vector2(Projectile.spriteDirection, 1f);
            Main.EntitySpriteDraw(
                ballTexture,
                Projectile.Center + offset - Main.screenPosition,
                new Rectangle(0, 0, 16, 16),
                lightColor * progress,
                MathHelper.Lerp(0, MathHelper.ToRadians(417f), EaseFunctions.EaseOutCubic(progress)),
                new Vector2(8, 8),
                1f, SpriteEffects.None
            );
        }

        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        if (SpecialTimer >= WIND_UP_TIME)
            DrawTrail(texture, lightColor);
        DrawSword(texture, Projectile.rotation, lightColor);
    }
}

public class BaseballBall : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 3 * 60;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
    }

    protected const int TRAIL_LENGTH = 8;
    
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    protected const float GRAVITY = 0.175f;
    protected const int FRAMES_AMOUNT = 14;
    protected const int FRAMES_HEIGHT = 16;
    protected ref float FrameCounter => ref Projectile.ai[0];
    
    public override void AI()
    {
        Projectile.velocity.Y += GRAVITY;
        FrameCounter = (FrameCounter + 0.25f) % FRAMES_AMOUNT;
        
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (oldVelocity.X.CompareTo(Projectile.velocity.X) != 0)
            Projectile.velocity.X = -oldVelocity.X * 0.9f;
        if (oldVelocity.Y.CompareTo(Projectile.velocity.Y) != 0)
            Projectile.velocity.Y = -oldVelocity.Y * 0.9f;
        
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
        return false;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f / FRAMES_AMOUNT);
        int currentFrame = (int)FrameCounter;
        Rectangle currentRect = new Rectangle(0, currentFrame * FRAMES_HEIGHT, texture.Width, texture.Height / FRAMES_AMOUNT);

        float scale = Projectile.timeLeft > 15 ? 1f : Projectile.timeLeft / 15f;
        
        for (int i = 0; i < TRAIL_LENGTH; i++)
        {
            int frame = (currentFrame - 1 + FRAMES_AMOUNT) % FRAMES_AMOUNT;
            Rectangle rect = new Rectangle(0, frame * FRAMES_HEIGHT, texture.Width, texture.Height / FRAMES_AMOUNT);
            Main.EntitySpriteDraw(
                texture,
                Projectile.oldPos[i] + origin - Main.screenPosition,
                rect,
                lightColor.WithAlpha(0) * (1f - i / (float)FRAMES_AMOUNT) * 0.75f,
                Projectile.oldRot[i],
                origin,
                0.9f * (1f - i / (float)FRAMES_AMOUNT) * scale,
                SpriteEffects.None
            );
        }
        
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            currentRect,
            lightColor,
            Projectile.rotation,
            origin,
            scale,
            SpriteEffects.None
        );
        return false;
    }
}