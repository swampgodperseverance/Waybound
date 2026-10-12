using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Ranged.Guns.PreHM
{
    //War guns abstract
    public enum Phase
    {
        None,
        Sweep,
        Finish,
        Jam
    }

    public class GunState
    {
        public int Mag;
        public int FireMode;
        public Phase Phase;
        public int PhaseTimer;
        public int FinishTime;
        public float ZoneCenter = 0.65f;
        public bool Boosted;
        public bool PendingBoost;
        public float Spread;
        public int SpreadCooldown;
        public float Meter;
        public float Climb;
        public int KickTimer;

        public GunState(int mag)
        {
            Mag = mag;
        }
    }

    public abstract class WarGun : ModItem
    {
        public const string Folder = "Waybound/Content/Items/Weapons/Ranged/Guns/PreHM/";

        public readonly struct ModeInfo
        {
            public readonly int UseTime;
            public readonly int UseAnimation;
            public readonly int ReuseDelay;
            public readonly bool AutoReuse;
            public readonly float Damage;
            public readonly int Pierce;

            public ModeInfo(int useTime, int useAnimation, int reuseDelay, bool autoReuse, float damage, int pierce)
            {
                UseTime = useTime;
                UseAnimation = useAnimation;
                ReuseDelay = reuseDelay;
                AutoReuse = autoReuse;
                Damage = damage;
                Pierce = pierce;
            }
        }

        private static readonly Dictionary<int, Texture2D[]> splitCache = new Dictionary<int, Texture2D[]>();

        public static bool TryGetSplit(int type, out Texture2D body, out Texture2D mag)
        {
            if (!splitCache.TryGetValue(type, out Texture2D[] pair))
            {
                pair = new Texture2D[2];
                string name = ModContent.GetModItem(type).Name;

                if (ModContent.HasAsset(Folder + name + "_Body") && ModContent.HasAsset(Folder + name + "_Mag"))
                {
                    pair[0] = ModContent.Request<Texture2D>(Folder + name + "_Body", AssetRequestMode.ImmediateLoad).Value;
                    pair[1] = ModContent.Request<Texture2D>(Folder + name + "_Mag", AssetRequestMode.ImmediateLoad).Value;
                }

                splitCache[type] = pair;
            }

            body = pair[0];
            mag = pair[1];
            return body != null && mag != null;
        }

        public override void Unload()
        {
            splitCache.Clear();
        }

        public static void Spark(Vector2 pos, Vector2 vel, float size, Color color, int life)
        {
            if (Main.dedServ)
                return;

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                color,
                life
            ));
        }

        public static void Star(Vector2 world, float rot, Vector2 scale, Color color)
        {
            Texture2D t = TextureAssets.Extra[98].Value;
            Main.EntitySpriteDraw(t, world - Main.screenPosition, null, color, rot, t.Size() / 2f, scale, SpriteEffects.None, 0);
        }

        public abstract int MagSize { get; }
        public abstract int ModeCount { get; }
        public abstract ModeInfo Mode(int mode);

        public virtual int SweepTime => 54;
        public virtual int JamTime => 110;
        public virtual float GoodHalf => 0.11f;
        public virtual float PerfectHalf => 0.035f;
        public virtual int FinishPerfect => 8;
        public virtual int FinishGood => 16;

        public virtual bool ModesOnRightClick => true;
        public virtual bool OneHanded => false;
        public virtual float HoldDistance => 22f;
        public virtual float MuzzleDistance => 46f;
        public virtual float KickAngle => 0.2f;
        public virtual int KickTime => 8;
        public virtual float MagDrop => 26f;
        public virtual float AmmoSave => 0f;
        public virtual Color Tracer => new Color(255, 190, 60);
        public virtual int[] ModeBars => new[] { 1, 3, 6 };
        public virtual string ExtraTip => null; //useless but ill keep

        public virtual Color[] ModeColors => new[]
        {
            new Color(255, 200, 90),
            new Color(120, 220, 255),
            new Color(255, 110, 90)
        };

        public virtual SoundStyle ShotSound => SoundID.Item11 with { Volume = 0.6f, PitchVariance = 0.08f };

        public virtual int UseTimeFor(GunState s, ModeInfo m) => m.UseTime;
        public virtual float ComputeSpread(Player player, GunState s) => 0f;
        public virtual float DamageMult(Player player, GunState s) => 1f;
        public virtual void OnShot(Player player, GunState s) { }
        public virtual void UpdateHeld(Player player, GunState s) { }
        public virtual void ModifyMovement(Player player, GunState s) { }
        public virtual void ModifyBulletHit(NPC target, ref NPC.HitModifiers modifiers, bool boosted) { }
        public virtual float MeterValue(GunState s) => -1f;
        public virtual Color MeterColor => new Color(120, 220, 255);

        public virtual Vector2 AdjustAim(Player player, GunState s, Vector2 dir)
        {
            float spread = ComputeSpread(player, s);
            return dir.RotatedBy(Main.rand.NextFloat(-spread, spread));
        }

        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 22;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.autoReuse = true;
            Item.knockBack = 2.5f;
            Item.shoot = ModContent.ProjectileType<WBullet>();
            Item.shootSpeed = 9f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override bool AltFunctionUse(Player player) => ModesOnRightClick && ModeCount > 1;

        public override void HoldItem(Player player)
        {
            int held = ModContent.ProjectileType<WarHeld>();
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[held] == 0)
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.UnitX, held, 0, 0f, player.whoAmI);
        }

        private void ApplyMode(GunState s)
        {
            ModeInfo m = Mode(s.FireMode);
            int ut = UseTimeFor(s, m);
            Item.useTime = ut;
            Item.useAnimation = m.UseAnimation == m.UseTime ? ut : m.UseAnimation;
            Item.reuseDelay = m.ReuseDelay;
            Item.autoReuse = m.AutoReuse;
        }

        public override bool CanUseItem(Player player)
        {
            WPlayer rp = player.GetModPlayer<WPlayer>();
            GunState s = rp.StateOf(this);

            if (player.altFunctionUse == 2)
            {
                if (player.whoAmI == Main.myPlayer && Main.mouseRightRelease)
                    rp.CycleMode(this);
                return false;
            }

            if (s.Phase != Phase.None || s.Mag <= 0)
                return false;

            ApplyMode(s);
            return true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            GunState s = player.GetModPlayer<WPlayer>().StateOf(this);
            if (s.Phase != Phase.None || s.Mag <= 0)
                return false;

            return Main.rand.NextFloat() >= AmmoSave;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            WPlayer rp = player.GetModPlayer<WPlayer>();
            GunState s = rp.StateOf(this);
            if (s.Phase != Phase.None || s.Mag <= 0)
                return false;

            ModeInfo m = Mode(s.FireMode);
            float mult = m.Damage * DamageMult(player, s) * (s.Boosted ? 1.3f : 1f);

            Vector2 dir = AdjustAim(player, s, velocity.SafeNormalize(Vector2.UnitX));
            Vector2 muzzle = player.MountedCenter + dir * MuzzleDistance;
            if (!Collision.CanHit(player.MountedCenter, 0, 0, muzzle, 0, 0))
                muzzle = player.MountedCenter;

            Projectile.NewProjectile(source, muzzle, dir * velocity.Length(), ModContent.ProjectileType<WBullet>(), (int)(damage * mult), knockback, player.whoAmI, s.Boosted ? 1f : 0f, Type, m.Pierce);

            s.Mag--;
            s.KickTimer = KickTime;
            s.SpreadCooldown = 10;
            OnShot(player, s);

            SoundEngine.PlaySound(ShotSound, player.Center);

            Color flash = s.Boosted ? new Color(255, 190, 70, 200) : new Color(255, 225, 150, 190);
            for (int i = 0; i < 3; i++)
                Spark(muzzle, dir.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 6f), Main.rand.NextFloat(10f, 16f), flash, Main.rand.Next(10, 18));

            if (s.Mag <= 0)
                rp.BeginReload(this, s);

            return false;
        }
    
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

        }
    }

    public class WPlayer : ModPlayer
    {
        public readonly Dictionary<int, GunState> States = new Dictionary<int, GunState>();
    public int FlashTimer;
    public Color FlashColor = Color.White;

    public float MagMult = 1f;
    public float SweepMult = 1f;
    public bool LastRounds;

    private bool wasUsing;
    private bool needRelease;

    public int MagFor(WarGun gun) => Math.Max(1, (int)Math.Round(gun.MagSize * MagMult));

    public int SweepFor(WarGun gun) => Math.Max(10, (int)(gun.SweepTime * SweepMult));

    public override void ResetEffects()
    {
        MagMult = 1f;
        SweepMult = 1f;
        LastRounds = false;
    }

    public GunState StateOf(WarGun gun)
    {
        if (!States.TryGetValue(gun.Type, out GunState s))
        {
            s = new GunState(gun.MagSize);
            States[gun.Type] = s;
        }
        return s;
    }

    public bool Active(out WarGun gun, out GunState state)
    {
        gun = null;
            state = null;

        foreach (KeyValuePair<int, GunState> pair in States)
        {
            if (pair.Value.Phase == Phase.None)
                continue;

            gun = ModContent.GetModItem(pair.Key) as WarGun;
            state = pair.Value;

            if (Player.HeldItem.type == pair.Key)
                break;
        }

        return state != null;
    }

    public void CycleMode(WarGun gun)
    {
        GunState s = StateOf(gun);
        s.FireMode = (s.FireMode + 1) % gun.ModeCount;
        s.Spread = 0f;
        s.Climb = 0f;
        SoundEngine.PlaySound(SoundID.MenuTick, Player.Center);
    }

    public void BeginReload(WarGun gun, GunState s)
    {
        s.Phase = Phase.Sweep;
        s.PhaseTimer = 0;
        s.ZoneCenter = Main.rand.NextFloat(0.5f, 0.82f);
        s.PendingBoost = false;
        s.Boosted = false;
        s.Spread = 0f;
        s.Climb = 0f;
        needRelease = true;
        SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.6f, Pitch = 0.3f }, Player.Center);
    }

    private void Flash(string text, Color color)
    {
        FlashColor = color;
        FlashTimer = 40;
        CombatText.NewText(Player.getRect(), color, text, true);
    }

    private void Resolve(WarGun gun, GunState s)
    {
        float marker = MathHelper.Clamp(s.PhaseTimer / (float)SweepFor(gun), 0f, 1f);
        float d = Math.Abs(marker - s.ZoneCenter);

        if (d <= gun.PerfectHalf)
        {
            s.PendingBoost = true;
            s.Phase = Phase.Finish;
            s.FinishTime = gun.FinishPerfect;
            s.PhaseTimer = 0;
            Flash("GOD DAMN", new Color(255, 215, 70));
            SoundEngine.PlaySound(SoundID.Item35 with { Volume = 0.7f, Pitch = 0.6f }, Player.Center);
            for (int i = 0; i < 10; i++)
                WarGun.Spark(Player.Top + new Vector2(0f, -28f), Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(10f, 16f), new Color(255, 215, 70, 200), Main.rand.Next(14, 24));
        }
        else if (d <= gun.GoodHalf)
        {
            s.Phase = Phase.Finish;
            s.FinishTime = gun.FinishGood;
            s.PhaseTimer = 0;
            Flash("NICE", new Color(110, 205, 255));
            SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.6f, Pitch = 0.5f }, Player.Center);
        }
        else
            Fail(s);
    }

    private void Fail(GunState s)
    {
        s.Phase = Phase.Jam;
        s.PhaseTimer = 0;
        s.PendingBoost = false;
        Flash("OOPS", new Color(255, 80, 70));
        SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.8f, Pitch = -0.5f }, Player.Center);
    }

    private void Complete(WarGun gun, GunState s)
    {
        s.Mag = MagFor(gun);
        s.Boosted = s.PendingBoost;
        s.Phase = Phase.None;
        s.PhaseTimer = 0;
    }

    public override void PostUpdateRunSpeeds()
    {
        if (Player.HeldItem.ModItem is WarGun gun)
            gun.ModifyMovement(Player, StateOf(gun));
    }

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (FlashTimer > 0)
            FlashTimer--;

        WarGun heldGun = Player.dead ? null : Player.HeldItem.ModItem as WarGun;
        bool using_ = Player.controlUseItem;
        bool press = using_ && !wasUsing;
        wasUsing = using_;

        if (needRelease)
        {
            if (!using_)
                needRelease = false;
            press = false;
        }

        foreach (KeyValuePair<int, GunState> pair in States)
        {
            WarGun gun = ModContent.GetModItem(pair.Key) as WarGun;
            if (gun == null)
                continue;

            GunState s = pair.Value;
            s.Mag = Math.Min(s.Mag, MagFor(gun));
            bool holding = heldGun != null && heldGun.Type == pair.Key;

            if (s.KickTimer > 0)
                s.KickTimer--;

            if (holding)
            {
                if (s.SpreadCooldown > 0)
                    s.SpreadCooldown--;
                else
                {
                    s.Spread = Math.Max(0f, s.Spread - 0.006f);
                    s.Climb = Math.Max(0f, s.Climb - 0.01f);
                }

                gun.UpdateHeld(Player, s);

                if (s.Mag <= 0 && s.Phase == Phase.None)
                    BeginReload(gun, s);
            }

            switch (s.Phase)
            {
                case Phase.Sweep:
                    if (!holding)
                    {
                        Fail(s);
                        break;
                    }

                    s.PhaseTimer++;
                    if (press)
                        Resolve(gun, s);
                    else if (s.PhaseTimer >= SweepFor(gun))
                        Fail(s);
                    break;

                case Phase.Finish:
                    s.PhaseTimer++;
                    if (s.PhaseTimer >= s.FinishTime)
                        Complete(gun, s);
                    break;

                case Phase.Jam:
                    s.PhaseTimer++;
                    if (s.PhaseTimer >= gun.JamTime)
                        Complete(gun, s);
                    break;
            }
        }
    }
}


public class WarHeld : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private float tilt;
        private Vector2 aim = Vector2.UnitX;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 58;
            Projectile.height = 22;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            WarGun gun = player.HeldItem.ModItem as WarGun;
            if (player.dead || !player.active || gun == null)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;

            GunState s = player.GetModPlayer<WPlayer>().StateOf(gun);

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 now = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                if (Vector2.Dot(now, Projectile.velocity.SafeNormalize(Vector2.UnitX)) < 0.9995f)
                {
                    Projectile.velocity = now;
                    Projectile.netUpdate = true;
                }
            }

            Vector2 mouseDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            player.ChangeDir(mouseDir.X > 0f ? 1 : -1);

            tilt = MathHelper.Lerp(tilt, s.Phase != Phase.None ? 1f : 0f, 0.18f);

            Vector2 direction = mouseDir.RotatedBy(player.direction * tilt * 0.45f);
            aim = direction;

            Projectile.direction = player.direction;
            Projectile.spriteDirection = player.direction;

            float holdDistance = MathHelper.Lerp(gun.HoldDistance, gun.HoldDistance - 4f, tilt);
            Projectile.Center = player.MountedCenter + direction * holdDistance;
            float baseRotation = direction.ToRotation() + (player.direction == -1 ? MathHelper.Pi : 0f);

            float kick = EaseFunctions.EaseIn(MathHelper.Clamp(s.KickTimer / (float)gun.KickTime, 0f, 1f)) * gun.KickAngle;

            Projectile.rotation = baseRotation - player.direction * kick;
            Vector2 up = direction.RotatedBy(-player.direction * MathHelper.PiOver2);
            Projectile.Center += up * (kick * 4.5f);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction - kick * 0.65f * player.direction);

            if (!gun.OneHanded)
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction * 0.85f - kick * 0.35f * player.direction + tilt * 0.5f * player.direction);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            WarGun gun = player.HeldItem.ModItem as WarGun;
            if (gun == null)
                return false;

            GunState s = player.GetModPlayer<WPlayer>().StateOf(gun);
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            bool split = WarGun.TryGetSplit(gun.Type, out Texture2D body, out Texture2D mag);
            Texture2D main = split ? body : TextureAssets.Item[gun.Type].Value;
            Vector2 origin = main.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(main, pos, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            if (split)
            {
                float off = 0f;
                float alpha = 1f;

                switch (s.Phase)
                {
                    case Phase.Sweep:
                        if (s.PhaseTimer < 14)
                        {
                            float k = s.PhaseTimer / 14f;
                            off = EaseFunctions.EaseInCubic(k);
                            alpha = 1f - k;
                        }
                        else
                            alpha = 0f;
                        break;

                    case Phase.Finish:
                        {
                            float k = s.PhaseTimer / (float)Math.Max(s.FinishTime, 1);
                            off = 1f - EaseFunctions.EaseOutCubic(k);
                            alpha = Math.Min(1f, k * 3f);
                            break;
                        }

                    case Phase.Jam:
                        {
                            int t = s.PhaseTimer;
                            int start = gun.JamTime - 18;
                            if (t < 14)
                            {
                                float k = t / 14f;
                                off = EaseFunctions.EaseInCubic(k);
                                alpha = 1f - k;
                            }
                            else if (t < start)
                                alpha = 0f;
                            else
                            {
                                float k = (t - start) / 18f;
                                off = 1f - EaseFunctions.EaseOutCubic(k);
                                alpha = Math.Min(1f, k * 3f);
                            }
                            break;
                        }
                }

                if (alpha > 0.01f)
                {
                    Vector2 magOff = new Vector2(0f, off * gun.MagDrop * Projectile.scale).RotatedBy(Projectile.rotation);
                    Main.EntitySpriteDraw(mag, pos + magOff, null, lightColor * alpha, Projectile.rotation, mag.Size() / 2f, Projectile.scale, effects, 0);
                }
            }

            if (s.KickTimer >= gun.KickTime - 1)
            {
                Vector2 muzzle = Projectile.Center + aim * (main.Width * 0.5f * Projectile.scale + 2f);
                Color c = s.Boosted ? new Color(255, 190, 70, 0) : new Color(255, 225, 150, 0);
                WarGun.Star(muzzle, Main.rand.NextFloat(MathHelper.TwoPi), new Vector2(0.45f), c);
                WarGun.Star(muzzle, aim.ToRotation(), new Vector2(0.8f, 0.22f), c);
            }

            return false;
        }
    }

    public class WBullet : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        private bool Boosted => Projectile.ai[0] == 1f;
        private WarGun Gun => ModContent.GetModItem((int)Projectile.ai[1]) as WarGun;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.extraUpdates = 2;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.penetrate = Math.Max(1, (int)Projectile.ai[2]);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Gun?.ModifyBulletHit(target, ref modifiers, Boosted);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Color t = Gun?.Tracer ?? new Color(255, 190, 60);
            Vector3 light = t.ToVector3() * (Boosted ? 0.7f : 0.3f);
            Lighting.AddLight(Projectile.Center, light);

            if (Boosted && Main.rand.NextBool(6))
                WarGun.Spark(Projectile.Center, Projectile.velocity * -0.05f + Main.rand.NextVector2Circular(0.6f, 0.6f), Main.rand.NextFloat(8f, 12f), new Color(t.R, t.G, t.B, 190), Main.rand.Next(12, 20));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.85f);

            for (int i = 0; i < 3; i++)
                WarGun.Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), Main.rand.NextFloat(9f, 14f), new Color(255, 215, 130, 190), Main.rand.Next(12, 20));
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
                WarGun.Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), Main.rand.NextFloat(9f, 14f), new Color(255, 215, 130, 190), Main.rand.Next(12, 20));

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Texture2D streak = TextureAssets.Extra[98].Value;
            bool boosted = Boosted;
            Color t = Gun?.Tracer ?? new Color(255, 190, 60);

            int tailIndex = boosted ? Projectile.oldPos.Length - 1 : 3;
            Vector2 tail = Projectile.Center;
            for (int k = tailIndex; k >= 1; k--)
            {
                if (Projectile.oldPos[k] != Vector2.Zero)
                {
                    tail = Projectile.oldPos[k] + Projectile.Size / 2f;
                    break;
                }
            }

            float len = Vector2.Distance(Projectile.Center, tail);
            if (len > 2f)
            {
                float rot = (Projectile.Center - tail).ToRotation() + MathHelper.PiOver2;
                Rectangle src = new Rectangle(0, streak.Height / 2, streak.Width, streak.Height / 2);
                Vector2 org = new Vector2(streak.Width * 0.5f, 0f);
                float scaleY = len / (streak.Height * 0.5f) * (boosted ? 1.4f : 1f);
                Color c = boosted ? new Color(t.R, t.G, t.B, 0) : new Color(255, 225, 150, 0);

                Main.EntitySpriteDraw(streak, Projectile.Center - Main.screenPosition, src, c * (boosted ? 0.8f : 0.45f), rot, org,
                    new Vector2(boosted ? 0.9f : 0.5f, scaleY), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(streak, Projectile.Center - Main.screenPosition, src, c * 0.35f, rot, org,
                    new Vector2(boosted ? 1.6f : 0.9f, scaleY * 0.8f), SpriteEffects.None, 0);
            }

            if (boosted)
                WarGun.Star(Projectile.Center, Projectile.rotation, new Vector2(0.35f, 0.8f), new Color(t.R, t.G, t.B, 0));

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class WargunLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player p = drawInfo.drawPlayer;
            if (p.whoAmI != Main.myPlayer || p.dead)
                return false;

            WPlayer rp = p.GetModPlayer<WPlayer>();
            return rp.FlashTimer > 0 || rp.Active(out _, out _);
        }

        private static void Rect(List<DrawData> cache, Vector2 pos, float w, float h, Color color, float rot = 0f)
        {
            cache.Add(new DrawData(TextureAssets.MagicPixel.Value, pos, new Rectangle(0, 0, 1, 1), color, rot, new Vector2(0.5f, 0.5f), new Vector2(w, h), SpriteEffects.None, 0f));
        }

        private static void Frame(List<DrawData> cache, Vector2 pos, float w, float h, float th, Color color)
        {
            Rect(cache, pos + new Vector2(0f, -h / 2f), w + th, th, color);
            Rect(cache, pos + new Vector2(0f, h / 2f), w + th, th, color);
            Rect(cache, pos + new Vector2(-w / 2f, 0f), th, h + th, color);
            Rect(cache, pos + new Vector2(w / 2f, 0f), th, h + th, color);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            WPlayer rp = player.GetModPlayer<WPlayer>();
            List<DrawData> cache = drawInfo.DrawDataCache;
            float time = Main.GlobalTimeWrappedHourly;
            Color green = new Color(70, 255, 110, 0);

            bool active = rp.Active(out WarGun gun, out GunState s);

            float a = active ? 1f : EaseFunctions.EaseOutQuad(MathHelper.Clamp(rp.FlashTimer / 20f, 0f, 1f));
            float intro = active && s.Phase == Phase.Sweep ? MathHelper.Clamp(s.PhaseTimer / 8f, 0f, 1f) : 1f;
            float pop = EaseFunctions.EaseOutBack(intro);
            a *= EaseFunctions.EaseOutCubic(intro);

            const float W = 96f;
            const float H = 10f;
            Vector2 c = player.Top + new Vector2(0f, -34f + (1f - pop) * 10f + player.gfxOffY) - Main.screenPosition;
            float pulse = 0.8f + 0.2f * (float)Math.Sin(time * 8f);

            Frame(cache, c, W, H, 2f, new Color(210, 225, 255, 0) * (0.5f * a));

            if (!active)
            {
                Rect(cache, c, W, H, new Color(rp.FlashColor.R, rp.FlashColor.G, rp.FlashColor.B, 0) * (0.5f * a));
                return;
            }

            switch (s.Phase)
            {
                case Phase.Sweep:
                    {
                        float gw = gun.GoodHalf * 2f * W;
                        float pw = Math.Max(gun.PerfectHalf * 2f * W, 2f);
                        float zx = c.X + (s.ZoneCenter - 0.5f) * W;

                        Rect(cache, new Vector2(zx, c.Y), gw, H, new Color(80, 190, 255, 0) * (0.5f * a));
                        Rect(cache, new Vector2(zx, c.Y - H / 2f), gw, 2f, new Color(110, 210, 255, 0) * (0.9f * a * pulse));
                        Rect(cache, new Vector2(zx, c.Y + H / 2f), gw, 2f, new Color(110, 210, 255, 0) * (0.9f * a * pulse));
                        Rect(cache, new Vector2(zx, c.Y), pw, H, new Color(255, 215, 70, 0) * (0.95f * a));

                        float marker = MathHelper.Clamp(s.PhaseTimer / (float)gun.SweepTime, 0f, 1f);
                        float mx = c.X + (marker - 0.5f) * W;
                        Rect(cache, new Vector2(mx, c.Y), 6f, H + 10f, new Color(255, 255, 255, 0) * (0.25f * a));
                        Rect(cache, new Vector2(mx, c.Y), 2f, H + 10f, new Color(255, 255, 255, 0) * a);
                        Rect(cache, new Vector2(mx, c.Y - H / 2f - 9f), 5f, 5f, green * a, MathHelper.PiOver4);
                        break;
                    }

                case Phase.Finish:
                    Rect(cache, c, W, H, new Color(rp.FlashColor.R, rp.FlashColor.G, rp.FlashColor.B, 0) * (0.7f * a));
                    break;

                case Phase.Jam:
                    {
                        float k = s.PhaseTimer / (float)gun.JamTime;
                        float wk = W * k;
                        Rect(cache, new Vector2(c.X - W / 2f + wk / 2f, c.Y), wk, H, new Color(255, 70, 60, 0) * (0.8f * a));
                        Frame(cache, c, W, H, 2f, new Color(255, 90, 80, 0) * (0.7f * a * pulse));
                        break;
                    }
            }
        }
    }

    public class WHS : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int i = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (i < 0)
                return;

            layers.Insert(i, new LegacyGameInterfaceLayer("Waybound: Rhythm Gun HUD", () =>
            {
                DrawHud();
                return true;
            }, InterfaceScaleType.UI));
        }

        private static void Bar(int x, int y, int w, int h, Color c)
        {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x, y, w, h), c);
        }

        private static void DrawHud()
        {
            Player p = Main.LocalPlayer;
            if (Main.playerInventory || p.dead || !(p.HeldItem.ModItem is WarGun gun))
                return;

            GunState s = p.GetModPlayer<WPlayer>().StateOf(gun);
            Vector2 pos = Main.MouseScreen + new Vector2(24f, 22f);
            int x = (int)pos.X;
            int y = (int)pos.Y;

            int textX = x;
            if (gun.ModeCount > 1)
            {
                Color[] colors = gun.ModeColors;
                Color mc = colors[s.FireMode % colors.Length];
                int bars = gun.ModeBars[s.FireMode % gun.ModeBars.Length];

                switch (bars)
                {
                    case 1:
                        Bar(x + 10, y, 4, 12, mc);
                        break;
                    case 2:
                        for (int i = 0; i < 2; i++)
                            Bar(x + 6 + i * 8, y, 5, 12, mc);
                        break;
                    case 3:
                        for (int i = 0; i < 3; i++)
                            Bar(x + 4 + i * 6, y, 3, 12, mc);
                        break;
                    default:
                        for (int i = 0; i < 6; i++)
                            Bar(x + i * 4, y, 2, 12, mc);
                        break;
                }

                textX = x + 32;
            }

            string text = s.Phase != Phase.None ? "--" : s.Mag + "/" + gun.MagSize;
            int low = Math.Max(3, gun.MagSize / 6);
            Color tc = s.Boosted ? new Color(255, 215, 70) : s.Mag <= low && s.Phase == Phase.None ? new Color(255, 100, 90) : Color.White;
            Utils.DrawBorderString(Main.spriteBatch, text, new Vector2(textX, y - 2), tc, 0.8f);

            float meter = gun.MeterValue(s);
            if (meter >= 0f)
            {
                Color mc = gun.MeterColor;
                Bar(textX, y + 17, 46, 3, new Color(mc.R, mc.G, mc.B, 255) * 0.25f);
                Bar(textX, y + 17, (int)(46 * MathHelper.Clamp(meter, 0f, 1f)), 3, mc);
            }
        }
    }
}