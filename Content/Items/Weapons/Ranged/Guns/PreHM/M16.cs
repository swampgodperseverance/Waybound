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
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Ranged.Guns.PreHM
{
    public class M16 : ModItem
    {
        public const string m16tast = "Waybound/Content/Items/Weapons/Ranged/Guns/PreHM/"; //m16 texture assets

        private static Asset<Texture2D> bodyAsset;
        private static Asset<Texture2D> magAsset;
        private static bool bodyChecked;
        private static bool magChecked;

        public static Texture2D BodyTex
        {
            get
            {
                if (!bodyChecked)
                {
                    bodyChecked = true;
                    if (ModContent.HasAsset(m16tast + "M16_Body"))
                        bodyAsset = ModContent.Request<Texture2D>(m16tast + "M16_Body", AssetRequestMode.ImmediateLoad);
                }
                return bodyAsset?.Value;
            }
        }

        public static Texture2D MagTex
        {
            get
            {
                if (!magChecked)
                {
                    magChecked = true;
                    if (ModContent.HasAsset(m16tast + "M16_Mag"))
                        magAsset = ModContent.Request<Texture2D>(m16tast + "M16_Mag", AssetRequestMode.ImmediateLoad);
                }
                return magAsset?.Value;
            }
        }

        public override void Unload()
        {
            bodyAsset = null;
            magAsset = null;
            bodyChecked = false;
            magChecked = false;
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

        public override void SetDefaults()
        {
            Item.width = 58;
            Item.height = 22;
            Item.damage = 14;
            Item.crit = 4;
            Item.knockBack = 2.5f;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<M16Bullet>();
            Item.shootSpeed = 9f;
            Item.useAmmo = AmmoID.Bullet;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 3);
        }

        public override bool AltFunctionUse(Player player) => true;

        public override Vector2? HoldoutOffset() => new Vector2(-4f, 0f);

        public override void HoldItem(Player player)
        {
            int held = ModContent.ProjectileType<M16Held>();
            if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[held] == 0)
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.UnitX, held, 0, 0f, player.whoAmI);
        }

        private void ApplyMode(int mode)
        {  //Shout out muspelheim
            switch (mode)
            {
                case 0:
                    Item.useTime = 20;
                    Item.useAnimation = 20;
                    Item.reuseDelay = 0;
                    Item.autoReuse = false;
                    break;
                case 1:
                    Item.useTime = 4;
                    Item.useAnimation = 12;
                    Item.reuseDelay = 18;
                    Item.autoReuse = true;
                    break;
                default:
                    Item.useTime = 8;
                    Item.useAnimation = 8;
                    Item.reuseDelay = 0;
                    Item.autoReuse = true;
                    break;
            }
        }

        public override bool CanUseItem(Player player)
        {
            M16Player mp = player.GetModPlayer<M16Player>();

            if (player.altFunctionUse == 2)
            {
                if (player.whoAmI == Main.myPlayer && Main.mouseRightRelease)
                    mp.CycleMode();
                return false;
            }

            if (mp.Phase != M16Phase.None || mp.Mag <= 0)
                return false;

            ApplyMode(mp.FireMode);
            return true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            M16Player mp = player.GetModPlayer<M16Player>();
            return mp.Phase == M16Phase.None && mp.Mag > 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            M16Player mp = player.GetModPlayer<M16Player>();
            if (mp.Phase != M16Phase.None || mp.Mag <= 0)
                return false;

            int mode = mp.FireMode;
            float spread = mode == 0 ? 0f : mode == 1 ? 0.012f : mp.Spread;
            float dmgMult = mode == 0 ? 1.8f : mode == 1 ? 1f : 0.85f;
            if (mp.Boosted)
                dmgMult *= 1.3f;

            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
            Vector2 muzzle = player.MountedCenter + dir * 46f;
            if (!Collision.CanHit(player.MountedCenter, 0, 0, muzzle, 0, 0))
                muzzle = player.MountedCenter;

            Vector2 v = dir.RotatedBy(Main.rand.NextFloat(-spread, spread)) * velocity.Length();
            int pierce = mode == 0 ? 3 : 1;

            Projectile.NewProjectile(source, muzzle, v, ModContent.ProjectileType<M16Bullet>(), (int)(damage * dmgMult), knockback, player.whoAmI, mp.Boosted ? 1f : 0f, mode, pierce);

            mp.Mag--;
            mp.KickTimer = mode == 0 ? 12 : 8;

            if (mode == 2)
            {
                mp.Spread = Math.Min(mp.Spread + 0.011f, 0.13f);
                mp.SpreadCooldown = 10;
            }

            SoundEngine.PlaySound(SoundID.Item11 with { Volume = mode == 0 ? 0.9f : 0.6f, Pitch = mode == 0 ? -0.25f : mode == 2 ? 0.1f : 0f, PitchVariance = 0.08f }, player.Center);

            Color flash = mp.Boosted ? new Color(255, 190, 70, 200) : new Color(255, 225, 150, 190);
            for (int i = 0; i < 3; i++)
                Spark(muzzle, dir.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 6f), Main.rand.NextFloat(10f, 16f), flash, Main.rand.Next(10, 18));

            if (mp.Mag <= 0)
                mp.BeginReload();

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
        }

        public override void AddRecipes()
        {
        }
    }

    public enum M16Phase
    {
        None,
        Sweep,
        Finish,
        Jam
    }

    public class M16Player : ModPlayer
    {
        public const int MagSize = 30;
        public const int SweepTime = 54;
        public const int JamTime = 110;
        public const float GoodHalf = 0.11f;
        public const float PerfectHalf = 0.035f;

        public int Mag = MagSize;
        public int FireMode;
        public M16Phase Phase;
        public int PhaseTimer;
        public int FinishTime;
        public float ZoneCenter = 0.65f;
        public bool Boosted;
        public float Spread;
        public int SpreadCooldown;
        public int KickTimer;
        public int FlashTimer;
        public Color FlashColor = Color.White;

        private bool pendingBoost;
        private bool wasUsing;
        private bool needRelease;

        public float Marker => MathHelper.Clamp(PhaseTimer / (float)SweepTime, 0f, 1f);

        public void CycleMode()
        {
            FireMode = (FireMode + 1) % 3;
            Spread = 0f;
            SoundEngine.PlaySound(SoundID.MenuTick, Player.Center);
        }

        public void BeginReload()
        {
            Phase = M16Phase.Sweep;
            PhaseTimer = 0;
            ZoneCenter = Main.rand.NextFloat(0.5f, 0.82f);
            pendingBoost = false;
            Boosted = false;
            Spread = 0f;
            needRelease = true;
            SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.6f, Pitch = 0.3f }, Player.Center);
        }

        private void Flash(string text, Color color)
        {
            FlashColor = color;
            FlashTimer = 40;
            CombatText.NewText(Player.getRect(), color, text, true);
        }

        private void Resolve(float pos)
        {
            float d = Math.Abs(pos - ZoneCenter);

            if (d <= PerfectHalf)
            {
                pendingBoost = true;
                Phase = M16Phase.Finish;
                FinishTime = 8;
                PhaseTimer = 0;
                Flash("GOD DAMN", new Color(255, 215, 70));
                SoundEngine.PlaySound(SoundID.Item35 with { Volume = 0.7f, Pitch = 0.6f }, Player.Center);
                for (int i = 0; i < 10; i++)
                    M16.Spark(Player.Top + new Vector2(0f, -28f), Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextFloat(10f, 16f), new Color(255, 215, 70, 200), Main.rand.Next(14, 24));
            }
            else if (d <= GoodHalf)
            {
                Phase = M16Phase.Finish;
                FinishTime = 16;
                PhaseTimer = 0;
                Flash("NICE", new Color(110, 205, 255));
                SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.6f, Pitch = 0.5f }, Player.Center);
            }
            else
                Fail();
        }

        private void Fail()
        {
            Phase = M16Phase.Jam;
            PhaseTimer = 0;
            pendingBoost = false;
            Flash("OOPS", new Color(255, 80, 70));
            SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.8f, Pitch = -0.5f }, Player.Center);
        }

        private void Complete()
        {
            Mag = MagSize;
            Boosted = pendingBoost;
            Phase = M16Phase.None;
            PhaseTimer = 0;
        }

        public override void PostUpdate()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            if (FlashTimer > 0)
                FlashTimer--;
            if (KickTimer > 0)
                KickTimer--;

            bool holding = Player.HeldItem.type == ModContent.ItemType<M16>() && !Player.dead;
            bool using_ = Player.controlUseItem;
            bool press = using_ && !wasUsing;
            wasUsing = using_;

            if (needRelease)
            {
                if (!using_)
                    needRelease = false;
                press = false;
            }

            if (holding)
            {
                if (SpreadCooldown > 0)
                    SpreadCooldown--;
                else
                    Spread = Math.Max(0f, Spread - 0.006f);

                if (Mag <= 0 && Phase == M16Phase.None)
                    BeginReload();
            }

            switch (Phase)
            {
                case M16Phase.Sweep:
                    if (!holding)
                    {
                        Fail();
                        break;
                    }

                    PhaseTimer++;
                    if (press)
                        Resolve(Marker);
                    else if (PhaseTimer >= SweepTime)
                        Fail();
                    break;

                case M16Phase.Finish:
                    PhaseTimer++;
                    if (PhaseTimer >= FinishTime)
                        Complete();
                    break;

                case M16Phase.Jam:
                    PhaseTimer++;
                    if (PhaseTimer >= JamTime)
                        Complete();
                    break;
            }
        }
    }
    //should i make an abstract? Who knows, sooner i will
    public class M16Held : ModProjectile
    {
        public override string Texture => M16.m16tast + "M16";

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
            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<M16>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;

            M16Player mp = player.GetModPlayer<M16Player>();

            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 now = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
                if (Vector2.Dot(now, Projectile.velocity.SafeNormalize(Vector2.UnitX)) < 0.9995f)
                {
                    Projectile.velocity = now;
                    Projectile.netUpdate = true;
                }
            }
            //Vector2 mouseDir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
            Vector2 mouseDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            player.ChangeDir(mouseDir.X > 0f ? 1 : -1);

            tilt = MathHelper.Lerp(tilt, mp.Phase != M16Phase.None ? 1f : 0f, 0.18f);

            Vector2 direction = mouseDir.RotatedBy(player.direction * tilt * 0.45f);
            aim = direction;

            Projectile.direction = player.direction;
            Projectile.spriteDirection = player.direction;

            float holdDistance = MathHelper.Lerp(22f, 18f, tilt);
            Projectile.Center = player.MountedCenter + direction * holdDistance;
            float baseRotation = direction.ToRotation() + (player.direction == -1 ? MathHelper.Pi : 0f);

            float t = mp.KickTimer / 10f;
            float kick = t * t * 0.2f;

            Projectile.rotation = baseRotation - player.direction * kick;
            Vector2 up = direction.RotatedBy(-player.direction * MathHelper.PiOver2);
            Projectile.Center += up * (kick * 4.5f);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction - kick * 0.65f * player.direction);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction * 0.85f - kick * 0.35f * player.direction + tilt * 0.5f * player.direction);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            M16Player mp = player.GetModPlayer<M16Player>();
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D body = M16.BodyTex;
            Texture2D mag = M16.MagTex;
            bool split = body != null && mag != null;
            Texture2D main = split ? body : TextureAssets.Projectile[Type].Value;
            Vector2 origin = main.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(main, pos, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            if (split)
            {
                float off = 0f;
                float alpha = 1f;

                switch (mp.Phase)
                {
                    case M16Phase.Sweep:
                        if (mp.PhaseTimer < 14)
                        {
                            float k = mp.PhaseTimer / 14f;
                            off = k * k;
                            alpha = 1f - k;
                        }
                        else
                            alpha = 0f;
                        break;

                    case M16Phase.Finish:
                        {
                            float k = mp.PhaseTimer / (float)Math.Max(mp.FinishTime, 1);
                            off = (1f - k) * (1f - k);
                            alpha = Math.Min(1f, k * 3f);
                            break;
                        }

                    case M16Phase.Jam:
                        {
                            int t = mp.PhaseTimer;
                            int start = M16Player.JamTime - 18;
                            if (t < 14)
                            {
                                float k = t / 14f;
                                off = k * k;
                                alpha = 1f - k;
                            }
                            else if (t < start)
                                alpha = 0f;
                            else
                            {
                                float k = (t - start) / 18f;
                                off = (1f - k) * (1f - k);
                                alpha = Math.Min(1f, k * 3f);
                            }
                            break;
                        }
                }

                if (alpha > 0.01f)
                {
                    Vector2 magOff = new Vector2(0f, off * 26f * Projectile.scale).RotatedBy(Projectile.rotation);
                    Main.EntitySpriteDraw(mag, pos + magOff, null, lightColor * alpha, Projectile.rotation, mag.Size() / 2f, Projectile.scale, effects, 0);
                }
            }

            if (mp.KickTimer >= 8)
            {
                Vector2 muzzle = Projectile.Center + aim * (main.Width * 0.5f * Projectile.scale + 2f);
                Color c = mp.Boosted ? new Color(255, 190, 70, 0) : new Color(255, 225, 150, 0);
                float s = mp.FireMode == 0 ? 0.6f : 0.4f;
                M16.Star(muzzle, Main.rand.NextFloat(MathHelper.TwoPi), new Vector2(s), c);
                M16.Star(muzzle, aim.ToRotation(), new Vector2(s * 1.8f, s * 0.5f), c);
            }

            return false;
        }
    }

    public class M16Bullet : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        private bool Boosted => Projectile.ai[0] == 1f;

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

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Vector3 light = Boosted ? new Vector3(0.7f, 0.45f, 0.12f) : new Vector3(0.3f, 0.24f, 0.1f);
            Lighting.AddLight(Projectile.Center, light);

            if (Boosted && Main.rand.NextBool(6))
                M16.Spark(Projectile.Center, Projectile.velocity * -0.05f + Main.rand.NextVector2Circular(0.6f, 0.6f), Main.rand.NextFloat(8f, 12f), new Color(255, 190, 70, 190), Main.rand.Next(12, 20));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.85f);

            for (int i = 0; i < 3; i++)
                M16.Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), Main.rand.NextFloat(9f, 14f), new Color(255, 215, 130, 190), Main.rand.Next(12, 20));
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
                M16.Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), Main.rand.NextFloat(9f, 14f), new Color(255, 215, 130, 190), Main.rand.Next(12, 20));

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Texture2D streak = TextureAssets.Extra[98].Value;
            bool boosted = Boosted;

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
                Color c = boosted ? new Color(255, 185, 60, 0) : new Color(255, 225, 150, 0);

                Main.EntitySpriteDraw(streak, Projectile.Center - Main.screenPosition, src, c * (boosted ? 0.8f : 0.45f), rot, org,
                    new Vector2(boosted ? 0.9f : 0.5f, scaleY), SpriteEffects.None, 0);
                Main.EntitySpriteDraw(streak, Projectile.Center - Main.screenPosition, src, c * 0.35f, rot, org,
                    new Vector2(boosted ? 1.6f : 0.9f, scaleY * 0.8f), SpriteEffects.None, 0);
            }

            if (boosted)
                M16.Star(Projectile.Center, Projectile.rotation, new Vector2(0.35f, 0.8f), new Color(255, 200, 90, 0));

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class M16HudLayer : PlayerDrawLayer      //It is pretty much like cron, idea from fargos
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player p = drawInfo.drawPlayer;
            M16Player mp = p.GetModPlayer<M16Player>();
            return p.whoAmI == Main.myPlayer && !p.dead && (mp.Phase != M16Phase.None || mp.FlashTimer > 0);
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
            M16Player mp = player.GetModPlayer<M16Player>();
            List<DrawData> cache = drawInfo.DrawDataCache;
            float time = Main.GlobalTimeWrappedHourly;
            Color green = new Color(70, 255, 110, 0);

            if (mp.Phase != M16Phase.None || mp.FlashTimer > 0)
            {
                float a = mp.Phase != M16Phase.None ? 1f : MathHelper.Clamp(mp.FlashTimer / 20f, 0f, 1f);
                float intro = mp.Phase == M16Phase.Sweep ? MathHelper.Clamp(mp.PhaseTimer / 8f, 0f, 1f) : 1f;
                float ease = 1f - (1f - intro) * (1f - intro);
                a *= ease;

                const float W = 96f;
                const float H = 10f;
                Vector2 c = player.Top + new Vector2(0f, -34f + (1f - ease) * 10f + player.gfxOffY) - Main.screenPosition;
                float pulse = 0.8f + 0.2f * (float)Math.Sin(time * 8f);

                Frame(cache, c, W, H, 2f, new Color(210, 225, 255, 0) * (0.5f * a));

                switch (mp.Phase)
                {
                    case M16Phase.Sweep:
                        {
                            float gw = M16Player.GoodHalf * 2f * W;
                            float pw = Math.Max(M16Player.PerfectHalf * 2f * W, 2f);
                            float zx = c.X + (mp.ZoneCenter - 0.5f) * W;

                            Rect(cache, new Vector2(zx, c.Y), gw, H, new Color(80, 190, 255, 0) * (0.5f * a));
                            Rect(cache, new Vector2(zx, c.Y - H / 2f), gw, 2f, new Color(110, 210, 255, 0) * (0.9f * a * pulse));
                            Rect(cache, new Vector2(zx, c.Y + H / 2f), gw, 2f, new Color(110, 210, 255, 0) * (0.9f * a * pulse));
                            Rect(cache, new Vector2(zx, c.Y), pw, H, new Color(255, 215, 70, 0) * (0.95f * a));

                            float mx = c.X + (mp.Marker - 0.5f) * W;
                            Rect(cache, new Vector2(mx, c.Y), 6f, H + 10f, new Color(255, 255, 255, 0) * (0.25f * a));
                            Rect(cache, new Vector2(mx, c.Y), 2f, H + 10f, new Color(255, 255, 255, 0) * a);
                            Rect(cache, new Vector2(mx, c.Y - H / 2f - 9f), 5f, 5f, green * a, MathHelper.PiOver4);
                            break;
                        }

                    case M16Phase.Finish:
                        Rect(cache, c, W, H, new Color(mp.FlashColor.R, mp.FlashColor.G, mp.FlashColor.B, 0) * (0.7f * a));
                        break;

                    case M16Phase.Jam:
                        {
                            float k = mp.PhaseTimer / (float)M16Player.JamTime;
                            float wk = W * k;
                            Rect(cache, new Vector2(c.X - W / 2f + wk / 2f, c.Y), wk, H, new Color(255, 70, 60, 0) * (0.8f * a));
                            Frame(cache, c, W, H, 2f, new Color(255, 90, 80, 0) * (0.7f * a * pulse));
                            break;
                        }

                    default:
                        Rect(cache, c, W, H, new Color(mp.FlashColor.R, mp.FlashColor.G, mp.FlashColor.B, 0) * (0.5f * a));
                        break;
                }
            }
        }
    }

    public class M16HudSystem : ModSystem
    {
        private static readonly Color[] ModeColors =
        {
            new Color(255, 200, 90),
            new Color(120, 220, 255),
            new Color(255, 110, 90)
        };

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int i = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (i < 0)
                return;

            layers.Insert(i, new LegacyGameInterfaceLayer("Waybound: M16 Mode", () =>
            {
                DrawMode();
                return true;
            }, InterfaceScaleType.UI));
        }

        private static void Bar(int x, int y, int w, int h, Color c)
        {
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x, y, w, h), c);
        }

        private static void DrawMode()
        {
            Player p = Main.LocalPlayer;
            if (Main.playerInventory || p.dead || p.HeldItem.type != ModContent.ItemType<M16>())
                return;

            M16Player mp = p.GetModPlayer<M16Player>();
            Vector2 pos = Main.MouseScreen + new Vector2(24f, 22f);
            int x = (int)pos.X;
            int y = (int)pos.Y;
            Color mc = ModeColors[mp.FireMode];

            switch (mp.FireMode)
            {
                case 0:
                    Bar(x + 10, y, 4, 12, mc);
                    break;
                case 1:
                    for (int i = 0; i < 3; i++)
                        Bar(x + 4 + i * 6, y, 3, 12, mc);
                    break;
                default:
                    for (int i = 0; i < 6; i++)
                        Bar(x + i * 4, y, 2, 12, mc);
                    break;
            }

            string text = mp.Phase != M16Phase.None ? "--" : mp.Mag + "/" + M16Player.MagSize;
            Color tc = mp.Boosted ? new Color(255, 215, 70) : mp.Mag <= 5 && mp.Phase == M16Phase.None ? new Color(255, 100, 90) : Color.White;
            Utils.DrawBorderString(Main.spriteBatch, text, new Vector2(x + 32, y - 2), tc, 0.8f);
        }
    }
}