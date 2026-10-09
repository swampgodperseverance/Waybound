using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Waybound.Content.Items.Weapons.Melee.Heavy;
using Waybound.Helpers;
using Waybound.Resources;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Waybound.Content.Projectiles.Base;

/// <summary>
/// The base class for all heavy weapons' sword projectile. <br/>
/// If you want to add your own heavy weapon, check <see cref="BaseballBat"/> as a reference for that.
/// </summary>
public abstract class BaseHeavySword : ModProjectile
{
    #region Properties & Fields
    
    /// <summary>
    /// Representation of <see cref="Projectile.ai"/>[0] in <see cref="int"/> for writing and reading internal data. <br/>
    /// • 1-8 bits - <see cref="Combo"/>; <br/>
    /// • 9-20 bits - <see cref="TimeLeft"/>; <br/>
    /// • 21 bit - <see cref="Direction"/>; <br/>
    /// • 22 bit - <see cref="CurrentState"/>. <br/>
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
    /// <b>Note: TimeLeft should NOT be more than 4095.</b>
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
    protected virtual float SwordLength => 50f;
    
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
    
    #endregion
    
    protected const int HAND_OFFSET_X = 10;
    
    public override bool ShouldUpdatePosition() => false;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 1;
        
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

    protected Vector2 GetSwordEdgePosition() => Projectile.Center + GetSwordEdgeOffset(Projectile.rotation);
    
    protected Vector2 GetSwordEdgeOffset(float rotation)
    {
        float finalRotation = MathHelper.PiOver2 - rotation * Projectile.spriteDirection;
        return new Vector2((SwordLength + SwordHiltOffset) * Projectile.scale + HAND_OFFSET_X, 0f).RotatedBy(finalRotation);
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

        UpdateAngle();
        UpdateTrail();
        
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
    protected void UpdateAngle()
    {
        if (Direction == SwingDirection.Down)
            UpdateAngle(MaxAngle, MinAngle);
        else
            UpdateAngle(MinAngle, MaxAngle);
    }

    protected const float TRAIL_INTERPOLATION_STEP = MathHelper.Pi / 36;
    protected void UpdateTrail()
    {
        if (Trail == null)
            return;

        if (_needClearTrail)
        {
            _needClearTrail = false;
            Trail.Clear();
            return;
        }
        
        Trail.Update();
        
        if (TimeLeft <= ComboContinueTime)
            return;
        
        int direction = Math.Sign(Projectile.rotation - Projectile.oldRot[0]);
        float currentRotation = Projectile.oldRot[0] + TRAIL_INTERPOLATION_STEP * direction;

        bool iterated = false;
        while ((currentRotation < Projectile.rotation && direction == 1) ||
               (currentRotation > Projectile.rotation && direction == -1))
        {
            Trail.Add(GetSwordEdgeOffset(currentRotation));
            currentRotation += TRAIL_INTERPOLATION_STEP * direction;
            iterated = true;
        }
        if(!iterated)
            Trail.Add(GetSwordEdgeOffset(Projectile.rotation));
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
        UpdateAngle();
        ClearTrail();
        OnClick();
    }

    /// <summary>
    /// Additional behavior on continuing attack. 
    /// </summary>
    protected virtual void OnClick()
    {
        SoundEngine.PlaySound(Audio.Get("HeavyWeapon_Swing"), Projectile.Center);
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

    protected virtual void DefaultDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
        
        DrawTrail(lightColor);
        DrawSword(texture, Projectile.rotation, lightColor);
    }

    protected DrawHelper.RibbonTrail Trail;
    protected virtual void DrawTrail(Color lightColor)
    {
        if (Trail == null)
            InitializeTrail();
        
        Trail?.Draw(
            lightColor,
            Direction == SwingDirection.Up ^ Projectile.spriteDirection == -1,
            offset: Projectile.Center
        );
    }

    private bool _needClearTrail;
    protected void ClearTrail() => _needClearTrail = true;
    
    /// <summary>
    /// The method where you can (and probably should) initialize your custom swing trail. <br/>
    /// If trail wasn't initialized, then it just won't be drawn and updated at all.
    /// </summary>
    protected virtual void InitializeTrail()
    {
        Trail = new DrawHelper.RibbonTrail(10);
        Trail.Initialize();

        Trail.SetPasses(
            x => (
                -4f,
                Color.Lerp(Color.White * 0.6f, Color.Transparent, x * x),
                0f,
                Color.Lerp(Color.White * 0.8f, Color.Transparent, x * x),
                null
            ),
            x => (
                -32f,
                Color.Transparent,
                -4f,
                Color.Lerp(Color.White * 0.3f, Color.Transparent, x * x),
                null
            )
        );
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