using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns;

namespace Waybound.Content.Items.Weapons.Ranged.LaserGuns
{
    //Kinda inspired by roa UIs serork such a genius
    public static class CronPalette
    {
        public const int Count = 5;
        public const int Black = 2;

        public static readonly Color[] Bg =
        {
            new Color(176, 98, 78, 100),
            new Color(120, 126, 142, 100),
            new Color(48, 48, 55, 100),
            new Color(190, 108, 40, 100),
            new Color(150, 60, 235, 100)
        };

        public static readonly Color[] Around =
        {
            new Color(238, 176, 146, 100),
            new Color(216, 222, 236, 100),
            new Color(92, 92, 102, 100),
            new Color(248, 190, 90, 100),
            new Color(214, 150, 255, 100)
        };

        public static readonly Color[] Marker =
        {
            new Color(238, 176, 146),
            new Color(216, 222, 236),
            new Color(70, 70, 78),
            new Color(248, 190, 90),
            new Color(214, 150, 255)
        };

        public static readonly Vector3[] Light =
        {
            new Vector3(0.75f, 0.45f, 0.35f),
            new Vector3(0.6f, 0.62f, 0.7f),
            new Vector3(0.28f, 0.28f, 0.32f),
            new Vector3(0.8f, 0.5f, 0.2f),
            new Vector3(0.6f, 0.3f, 0.95f)
        };

        public static readonly int[] Dust =
        {
            DustID.Blood,
            DustID.Silver,
            DustID.Shadowflame,//should've been black
            DustID.Copper,
            DustID.PurpleTorch
        };
    }

    public class Cron : LaserGun
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/LaserGuns/Cron5";

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

            if (!Main.dedServ)
                Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, CronPalette.Count));
        }

        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.crit = -4;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 32;
            Item.height = 22;
            base.SetDefaults();
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0f;
            Item.value = Item.sellPrice(0, 0, 30, 0);
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item12;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<CronP>();
            Item.shootSpeed = 1f;
            chargeMax = 200;
            chargeAdd = 1;
            chargeRemove = 5;
        }

        public override void UpdateInventory(Player player)
        {
        }

        public override bool CanUseItem(Player player)
        {
            CronPlayer cron = player.GetModPlayer<CronPlayer>();
            if (cron.PickerOpen || cron.SelectCooldown > 0)
                return false;

            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, player.GetModPlayer<CronPlayer>().Variant);
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8, 1);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddRecipeGroup(RecipeGroupID.IronBar, 8)
                .AddIngredient(ItemID.Wire, 10)
                .AddIngredient(ItemID.Lens, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class CronP : RangedLaser
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private int Variant => Utils.Clamp((int)Projectile.ai[1], 0, CronPalette.Count - 1);

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.scale = 0.3f;
            Projectile.timeLeft = 2;
            moveDistance = 40f;
            moveSpeed = 2f;
            maxDistance = 240;
            noParticles = true;
            laserDust = CronPalette.Dust[0];
            colorLineBG = CronPalette.Bg[0];
            colorLinesAround = CronPalette.Around[0];
        }

        public override void AI()
        {
            int variant = Variant;
            colorLineBG = CronPalette.Bg[variant];
            colorLinesAround = CronPalette.Around[variant];
            laserDust = CronPalette.Dust[variant];
            base.AI();

            Player owner = Main.player[Projectile.owner];
            Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float length = Distance - moveDistance;
            if (length > 0f)
            {
                float charge = MathHelper.Clamp((Projectile.scale - 0.25f) / 0.75f, 0f, 1f);
                Vector3 light = CronPalette.Light[variant];
                Lighting.AddLight(owner.Center + unit * Distance, light * (0.8f + charge * 0.9f));
                Lighting.AddLight(owner.Center + unit * moveDistance, light * (0.5f + charge * 0.5f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.UnitX);

            DrawLaser(owner.Center + unit * moveDistance, Projectile.velocity, -MathHelper.PiOver2);
            return false;
        }
    }

    public class CronPlayer : ModPlayer
    {
        public int Variant;
        public bool PickerOpen;
        public float PickerFade;
        public int Hovered = -1;
        public int SelectCooldown;
        public readonly float[] Hover = new float[CronPalette.Count];

        private bool prevLeft;
        private bool prevRight;
        private int prevHovered = -1;

        public Vector2 GetSlotCenter(int index)
        {
            return Player.Bottom + new Vector2((index - (CronPalette.Count - 1) / 2f) * 46f, 38f);
        }

        public override void SaveData(TagCompound tag)
        {
            tag["cronVariant"] = Variant;
        }

        public override void LoadData(TagCompound tag)
        {
            Variant = Utils.Clamp(tag.GetInt("cronVariant"), 0, CronPalette.Count - 1);
        }

        public override void PostUpdate()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            if (!Main.dedServ)
            {
                var anim = Main.itemAnimations[ModContent.ItemType<Cron>()];
                if (anim != null)
                    anim.Frame = Variant;
            }

            bool holding = !Player.dead && Player.HeldItem.type == ModContent.ItemType<Cron>();
            bool left = Main.mouseLeft;
            bool right = Main.mouseRight;

            if (SelectCooldown > 0)
                SelectCooldown--;

            if (!holding)
            {
                PickerOpen = false;
            }
            else if (right && !prevRight && !Player.mouseInterface && !Main.mapFullscreen)
            {
                PickerOpen = !PickerOpen;
                SoundEngine.PlaySound(SoundID.MenuOpen);
            }

            Hovered = -1;
            if (PickerOpen && PickerFade > 0.55f)
            {
                for (int i = 0; i < CronPalette.Count; i++)
                {
                    Vector2 c = GetSlotCenter(i);
                    if (Math.Abs(Main.MouseWorld.X - c.X) < 22f && Math.Abs(Main.MouseWorld.Y - c.Y) < 24f)
                    {
                        Hovered = i;
                        break;
                    }
                }

                if (Hovered != -1 && Hovered != prevHovered)
                    SoundEngine.PlaySound(SoundID.MenuTick);

                if (Hovered != -1 && left && !prevLeft)
                {
                    Variant = Hovered;
                    PickerOpen = false;
                    SelectCooldown = 12;
                    SoundEngine.PlaySound(SoundID.Item35 with { Volume = 0.6f, Pitch = 0.4f }, Player.Center);
                }
            }

            PickerFade = MathHelper.Clamp(PickerFade + (PickerOpen ? 0.07f : -0.09f), 0f, 1f);

            for (int i = 0; i < CronPalette.Count; i++)
                Hover[i] = MathHelper.Lerp(Hover[i], Hovered == i ? 1f : 0f, 0.25f);

            prevHovered = Hovered;
            prevLeft = left;
            prevRight = right;
        }
    }

    public class CronPickerLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player p = drawInfo.drawPlayer;
            return p.whoAmI == Main.myPlayer && p.GetModPlayer<CronPlayer>().PickerFade > 0.01f;
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            CronPlayer cron = player.GetModPlayer<CronPlayer>();

            Texture2D tex = TextureAssets.Item[ModContent.ItemType<Cron>()].Value;
            Texture2D px = TextureAssets.MagicPixel.Value;
            Rectangle pxSrc = new Rectangle(0, 0, 1, 1);
            Vector2 half = new Vector2(0.5f, 0.5f);
            float time = Main.GlobalTimeWrappedHourly;

            Color green = new Color(70, 255, 110, 0);

            for (int i = 0; i < CronPalette.Count; i++)
            {
                float t = MathHelper.Clamp(cron.PickerFade * 1.9f - i * 0.18f, 0f, 1f);
                if (t <= 0f)
                    continue;

                float ease = 1f - (1f - t) * (1f - t);
                float hover = cron.Hover[i];

                Vector2 center = cron.GetSlotCenter(i) + new Vector2(0f, (1f - ease) * 14f - hover * 4f);
                Vector2 pos = center - Main.screenPosition;

                Rectangle src = tex.Frame(1, CronPalette.Count, 0, i);
                src.Height -= 2;
                Vector2 origin = src.Size() / 2f;
                float scale = (0.88f + ease * 0.12f) * (1f + hover * 0.18f);

                drawInfo.DrawDataCache.Add(new DrawData(px, pos, pxSrc, new Color(8, 14, 26) * (0.5f * ease), 0f, half,
                    new Vector2(44f, 44f), SpriteEffects.None, 0f));

                if (hover > 0.01f)
                {
                    float a = hover * ease * (0.8f + 0.2f * (float)Math.Sin(time * 8f));
                    Color line = green * a;
                    drawInfo.DrawDataCache.Add(new DrawData(px, pos + new Vector2(0f, -22f), pxSrc, line, 0f, half, new Vector2(46f, 2f), SpriteEffects.None, 0f));
                    drawInfo.DrawDataCache.Add(new DrawData(px, pos + new Vector2(0f, 22f), pxSrc, line, 0f, half, new Vector2(46f, 2f), SpriteEffects.None, 0f));
                    drawInfo.DrawDataCache.Add(new DrawData(px, pos + new Vector2(-22f, 0f), pxSrc, line, 0f, half, new Vector2(2f, 46f), SpriteEffects.None, 0f));
                    drawInfo.DrawDataCache.Add(new DrawData(px, pos + new Vector2(22f, 0f), pxSrc, line, 0f, half, new Vector2(2f, 46f), SpriteEffects.None, 0f));

                    for (int k = 0; k < 4; k++)
                    {
                        Vector2 off = new Vector2(2f, 0f).RotatedBy(MathHelper.PiOver2 * k);
                        drawInfo.DrawDataCache.Add(new DrawData(tex, pos + off, src, green * (hover * ease), 0f, origin, scale, SpriteEffects.None, 0f));
                    }
                }

                drawInfo.DrawDataCache.Add(new DrawData(tex, pos, src, Color.White * ease, 0f, origin, scale, SpriteEffects.None, 0f));

                if (i == cron.Variant)
                {
                    drawInfo.DrawDataCache.Add(new DrawData(px, pos + new Vector2(0f, 29f), pxSrc, CronPalette.Marker[i] * ease, MathHelper.PiOver4, half,
                        new Vector2(5f, 5f), SpriteEffects.None, 0f));
                }
            }
        }
    }
}