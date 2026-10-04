using System;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Waybound.Helpers;
using Waybound.Particles;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Waybound.Content.Projectiles.Base;

public abstract class BaseHeavySword : ModProjectile
{
    #region Properties & Fields
    
    /// <summary>
    /// Representation of <see cref="Projectile.ai"/>[0] in <see cref="int"/> for writing and reading internal data. <br/>
    /// • 1-8 bits - current combo; <br/>
    /// • 9-20 bits - time left (syncs automatically with <see cref="Projectile.timeLeft"/>; <br/>
    /// • 21 bit - direction (0 is up, 1 is down); <br/>
    /// • 22 bit - state (0 is default state, 1 is special state). <br/>
    /// 23-32 bits remain unused, so if your projectile needs to store some extra data
    /// (and Projectile.ai[1-2] is not enough for some reason), you can store it here.
    /// </summary>
    protected int Data
    {
        get => BitConverter.SingleToInt32Bits(Projectile.ai[0]);
        set => Projectile.ai[0] = BitConverter.Int32BitsToSingle(value);
    }

    /// <summary>
    /// Current combo score. Increases for each successful hit. <br/>
    /// Clamped from 0 to <b>MaxCombo</b>. <br/>
    /// Each heavy weapon can vary its behavior and/or add special attack based on this parameter. <br/>
    /// <b>Note: Combo should NOT be more than 255.</b>
    /// </summary>
    protected int Combo
    {
        get => BitwiseHelper.ReadBits(Data, 0, 8);
        set => Data = BitwiseHelper.WriteBits(Data, 0, 8, value);
    }

    /// <summary>
    /// Current direction of swing.
    /// </summary>
    protected SwingDirection Direction
    {
        get => (SwingDirection)(BitwiseHelper.ReadBits(Data, 20, 1) * 2 - 1);
        set => Data = BitwiseHelper.WriteBits(Data, 20, 1, ((int)value + 1) / 2);
    }
    public enum SwingDirection
    {
        Up = 1,
        Down = -1,
    }

    /// <summary>
    /// Remaining projectile lifetime that's synced with server and other clients. <br/>
    /// You <b>should</b> use it instead of <see cref="Projectile.timeLeft"/> (mostly because changing it will do nothing).
    /// </summary>
    protected int TimeLeft
    {
        get => BitwiseHelper.ReadBits(Data, 8, 12);
        set => Data = BitwiseHelper.WriteBits(Data, 8, 12,  value);
    }

    /// <summary>
    /// Current projectile behavior. It has two states: default and special. <br/>
    /// In special state, these methods are running instead of defaults:
    /// <see cref="AI_OnSpecial"/>, <see cref="Draw_OnSpecial"/>, <see cref="CanHit_OnSpecial"/>, 
    /// <see cref="Colliding_OnSpecial"/>
    /// </summary>
    protected AttackState CurrentState
    {
        get => (AttackState)BitwiseHelper.ReadBits(Data, 21, 1);
        set => Data = BitwiseHelper.WriteBits(Data, 21, 1, (int)value);
    }
    public enum AttackState
    {
        Default = 0,
        Special = 1,
    }
    
    /// <summary>
    /// LMB Buffer. If it's more than 0, it will trigger continuing combo.
    /// </summary>
    protected ref float ClickExpireStamp => ref Projectile.localAI[0];
    
    #endregion
    
    #region Overridable Configuration Parameters
    
    /// <summary>
    /// Max combo score. If set to 0, the combo counter will be disabled. <br/>
    /// <b>Note: MaxCombo should NOT be more than 255.</b>
    /// </summary>
    protected virtual int MaxCombo => 0;
    
    /// <summary>
    /// Final angle of projectile rotation. (0 = straight down, 180 = straight up)
    /// </summary>
    protected virtual float SwingMaxAngle => 260f;
    
    /// <summary>
    /// Length of the projectile hitbox.
    /// </summary>
    protected virtual float SwordLength => 46f;
    
    /// <summary>
    /// The angle (in degrees) of the sword on the sprite itself (0 = sword lying horizontally)
    /// </summary>
    protected virtual float SwordAngleOffset => 45f;
    
    /// <summary>
    /// The sword's hilt diagonal offset (in pixels) on the sprite itself (0 = bottom left corner)
    /// </summary>
    protected virtual float SwordHiltOffset => 4f;
    
    /// <summary>
    /// Width of the projectile's hitbox.
    /// </summary>
    protected virtual float HitboxSize => 8;
    
    /// <summary>
    /// Duration of a single sword swing without attack speed modifiers (in ticks).
    /// </summary>
    protected virtual int BaseSwingTime => 30;
    
    /// <summary>
    /// Duration of a single sword swing with attack speed modifiers (in ticks).
    /// </summary>
    protected int SwingTime => (int)Math.Round(BaseSwingTime / Owner.GetAttackSpeed<MeleeDamageClass>());
    
    /// <summary>
    /// Time after a swing in which player can press LMB/RMB to continue combo attack. (in ticks)
    /// </summary>
    protected virtual int ComboContinueTime => 15;

    /// <summary>
    /// Length of the projectile trail in <see cref="DrawTrail()"/>.
    /// </summary>
    protected virtual int TrailLength => 6;
    
    #endregion
    
    protected const int HAND_OFFSET_X = 10;
    
    public override bool ShouldUpdatePosition() => false;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        
        SetStaticDefaults_Extra();
    }
    protected virtual void SetStaticDefaults_Extra() {}

    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Melee;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.ownerHitCheck = true;
        Projectile.width = Projectile.height = (int)HitboxSize;
        Projectile.netImportant = true;
        
        SetDefaults_Extra();
    }
    protected virtual void SetDefaults_Extra() {}

    protected virtual float MinAngle => MathHelper.ToRadians(-75f);
    protected virtual float MaxAngle => MathHelper.ToRadians(SwingMaxAngle);
    
    public override void OnSpawn(IEntitySource source)
    {
        Direction = SwingDirection.Down;
        Projectile.rotation = MaxAngle;
        for(int i = 0; i < TrailLength; i++)
            Projectile.oldRot[i] = Projectile.rotation;
        TimeLeft = SwingTime + ComboContinueTime;
        
        OnSpawn_Extra(source);
    }
    protected virtual void OnSpawn_Extra(IEntitySource source) {}

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CurrentState == AttackState.Special)
            Colliding_OnSpecial(projHitbox, targetHitbox);
        
        return SwordColliding(targetHitbox);
    }
    protected virtual bool Colliding_OnSpecial(Rectangle projHitbox, Rectangle targetHitbox) => false;
    protected virtual bool SwordColliding(Rectangle targetHitbox)
    {
        float c = 0;
        return Collision.CheckAABBvLineCollision(
            targetHitbox.TopLeft(),
            targetHitbox.Size(),
            Projectile.Center,
            GetSwordEdgePosition(),
            HitboxSize,
            ref c
        );
    }
 
    public override bool? CanHitNPC(NPC target)
    {
        if (CurrentState == AttackState.Special)
            return CanHit_OnSpecial(target);

        return TimeLeft > ComboContinueTime;
    }
    protected virtual bool CanHit_OnSpecial(NPC target) => false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (MaxCombo > 0 && Combo < MaxCombo)
            Combo = Math.Clamp(Combo + 1, 0, MaxCombo);
        
        OnHitNPC_Extra(target, hit, damageDone);
    }
    protected virtual void OnHitNPC_Extra(NPC target, NPC.HitInfo hit, int damageDone) { }
    
    protected Vector2 GetSwordEdgePosition()
    {
        float finalRotation = MathHelper.PiOver2 - Projectile.rotation * Projectile.spriteDirection;
        return Projectile.Center + new Vector2((SwordLength + SwordHiltOffset) * Projectile.scale + HAND_OFFSET_X, 0f).RotatedBy(finalRotation);
    }

    protected Player Owner => Main.player[Projectile.owner];
    
    public override void AI()
    {
        if (Owner == null || Owner.DeadOrGhost)
        {
            Projectile.Kill();
            return;
        }
        
        Projectile.timeLeft = TimeLeft;
        
        Owner.itemAnimation = 2;
        Owner.itemTime = 2;
        Owner.heldProj = Projectile.whoAmI;
        
        Projectile.spriteDirection = Owner.direction;
        Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter);

        UpdateClickTimer();
        
        if (CurrentState == AttackState.Special)
        {
            AI_OnSpecial();
            return;
        }
        
        if (Main.myPlayer == Projectile.owner)
        {
            if (TimeLeft <= ComboContinueTime && ClickExpireStamp > 0)
            {
                ContinueAttack();
                Projectile.netUpdate = true;
            }

            if (CanUseSpecialAttack() && Main.mouseRightRelease && Main.mouseRight)
            {
                CastSpecialAttack();
                Projectile.netUpdate = true;
            }
        }

        if (Direction == SwingDirection.Down)
            UpdateAngle(MaxAngle, MinAngle);
        else
            UpdateAngle(MinAngle, MaxAngle);
        
        if(TimeLeft > ComboContinueTime + 10)
            SpawnParticles();
        
        Owner.SetCompositeArmFront(
            true,
            Player.CompositeArmStretchAmount.Full, 
            Projectile.rotation * -Projectile.spriteDirection
        );

        TimeLeft--;
    }
    protected virtual void AI_OnSpecial() {}
    
    protected void UpdateClickTimer()
    {
        if (Main.myPlayer != Projectile.owner) return;
        
        if (Main.mouseLeftRelease && Main.mouseLeft)
            ClickExpireStamp = 10;
        ClickExpireStamp--;
    }
    
    protected void UpdateAngle(float angleFrom, float angleTo)
    {
        if (TimeLeft < ComboContinueTime)
            return;
        
        float percentage = 1f - (TimeLeft - ComboContinueTime) / (float)SwingTime;
        Projectile.rotation = MathHelper.Lerp(angleFrom, angleTo, EaseFunctions.EaseOutCubic(percentage));
    }
    protected void UpdateAngle(float angleFrom, float angleTo, float progress)
    {
        Projectile.rotation = MathHelper.Lerp(angleFrom, angleTo, progress);
    }

    public override void OnKill(int timeLeft)
    {
        Owner.itemAnimation = 0;
        Owner.itemTime = 0;
        OnKill_Extra(timeLeft);
    }
    protected virtual void OnKill_Extra(int timeLeft) {}
    
    /// <summary>
    /// Decides if player can use special ability on RMB. Returns <b>false</b> by default. <br/>
    /// Here you should add your combo requirement for using special attack (e.g. combo == 5)
    /// or any other requirements you want.
    /// </summary>
    protected virtual bool CanUseSpecialAttack() => false;

    /// <summary>
    /// Decides what will happen on using special attack. <br/>
    /// If you want to disable default behavior of the projectile, change <see cref="CurrentState"/> to <see cref="AttackState.Special"/>.
    /// </summary>
    protected virtual void CastSpecialAttack() {}

    protected void ContinueAttack()
    {
        ClickExpireStamp = 0;
        Direction = (SwingDirection)((int)Direction * -1);
        TimeLeft = SwingTime + ComboContinueTime;
        Projectile.ResetLocalNPCHitImmunity();
        Owner.direction = Main.MouseWorld.X < Projectile.Center.X ? -1 : 1;
        OnClick();
        Projectile.netUpdate = true;
    }

    /// <summary>
    /// Additional behavior on continuing attack. 
    /// </summary>
    protected virtual void OnClick()
    {
        SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
    }

    #region Drawing
    
    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.myPlayer == Projectile.owner && MaxCombo > 0 && Combo > 0)
            DrawComboMeter();
        
        if (CurrentState == AttackState.Special)
        {
            Draw_OnSpecial(ref lightColor);
            return false;
        }
        
        DefaultDraw(ref lightColor);
        return false;
    }

    protected virtual void DrawComboMeter()
    {
        Color color = CanUseSpecialAttack()
            ? Color.Gold 
            : Color.WhiteSmoke;
        DrawHelper.DrawBar(
            Projectile.Center + new Vector2(0, Owner.height / 2f + 8),
            Combo,
            MaxCombo,
            color,
            1f
        );
    }

    protected virtual void SpawnParticles()
    {
        ParticleSystem.TrailBuffer.Create(new ParticleInfo(
            position: GetSwordEdgePosition().ToNumerics(),
            velocity: System.Numerics.Vector2.Zero,
            rotation: Projectile.rotation * -Projectile.spriteDirection,
            scale: new System.Numerics.Vector2(24f, 12f),
            color: new Color(255, 255, 255, 80),
            duration: 45
        ));
    }

    protected virtual void DefaultDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        
        DrawTrail(texture, lightColor);
        DrawSword(texture, Projectile.rotation, lightColor);
    }

    protected virtual void DrawTrail(Texture2D texture, Color lightColor)
    {
        for (int i = 0; i < TrailLength; i++)
        {
            float alpha = MathHelper.Clamp(0.75f - (float)i / TrailLength, 0, 1); 
            DrawSword(texture, Projectile.oldRot[i], lightColor * alpha * 0.6f);
        }
    }

    protected virtual void DrawSword(Texture2D texture, float rotation, Color lightColor)
    {
        float finalRotation = MathHelper.PiOver2 + MathHelper.ToRadians(SwordAngleOffset) - rotation;
        SpriteEffects spriteEffect = SpriteEffects.None;
        Vector2 origin = new Vector2(0, texture.Height) + SwordHiltOffset * new Vector2(1, -1);
        if (Projectile.spriteDirection == -1)
        {
            spriteEffect |= SpriteEffects.FlipHorizontally;
            origin.X = texture.Width - origin.X;
        }

        if (Direction == SwingDirection.Up)
        {
            spriteEffect |= SpriteEffects.FlipVertically;
            origin.Y = texture.Height - origin.Y;
            finalRotation -= MathHelper.PiOver2;
        }

        float offsetRotation = MathHelper.PiOver2 - rotation * Projectile.spriteDirection;
        Main.EntitySpriteDraw(
            texture,
            Projectile.Center + new Vector2(HAND_OFFSET_X, 0).RotatedBy(offsetRotation) - Main.screenPosition,
            null,
            lightColor,
            finalRotation * Projectile.spriteDirection,
            origin,
            Projectile.scale,
            spriteEffect
        );
    }
    protected virtual void Draw_OnSpecial(ref Color lightColor) {}
    
    #endregion
}