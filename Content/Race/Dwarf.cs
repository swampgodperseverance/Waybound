using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.ModSystems;

namespace Waybound.Content.Race;

public class Dwarf : RaceInfo
{
    protected override Stat SetStat => new(-20, 40);
    public override Color[] Colors => [new(120, 90, 255), new(60, 220, 255)];
}

public static class DwarfRace
{
    public static bool IsDwarf(Player player)
    {
        if (player is null || !player.active)
            return false;

        return Common.WUtils.Race.CheckRace(player, Common.WUtils.Race.ID.Dwarf);
    }
}

public enum SpaceShape
{
    Line,
    Triangle,
    Bridge,
    Step,
    Square,
    Hexagon,
    Circle,
    Diamond,
    Star,
    Plateau
}

public static class SpaceShapeData
{
    public const int Count = 10;

    public static readonly int[] Cooldown = { 40, 180, 130, 90, 120, 150, 150, 180, 1200, 300 };
    public static readonly int[] Life = { 70, 150, 600, 220, 240, 280, 300, 300, 360, 1200 };
    public static readonly int[] Damage = { 0, 0, 0, 0, 14, 0, 0, 16, 44, 0 };
    public static readonly float[] Size = { 70f, 52f, 110f, 28f, 60f, 62f, 72f, 64f, 84f, 200f };
    public static readonly float[] MiniSize = { 11f, 9f, 12f, 7f, 8f, 9f, 9f, 9f, 10f, 13f };

    public static readonly Color[] Colors =
    {
        new Color(140, 200, 255),
        new Color(255, 190, 110),
        new Color(120, 230, 255),
        new Color(190, 255, 150),
        new Color(140, 255, 200),
        new Color(110, 160, 255),
        new Color(200, 150, 255),
        new Color(255, 150, 150),
        new Color(255, 120, 220),
        new Color(255, 215, 120)
    };

    public static bool IsRectangle(int index) => index == (int)SpaceShape.Bridge || index == (int)SpaceShape.Plateau;

    public static float RectHalfHeight(SpaceShape shape, float size)
    {
        return shape == SpaceShape.Plateau ? size * 0.07f : Math.Max(size * 0.12f, 3f);
    }

    public static bool TryGetPlatform(SpaceShape shape, Vector2 center, float size, out float y, out float halfWidth)
    {
        y = 0f;
        halfWidth = 0f;

        switch (shape)
        {
            case SpaceShape.Line:
                y = center.Y;
                halfWidth = size;
                return true;
            case SpaceShape.Bridge:
            case SpaceShape.Plateau:
                y = center.Y - RectHalfHeight(shape, size);
                halfWidth = size;
                return true;
            case SpaceShape.Step:
                y = center.Y - size;
                halfWidth = size;
                return true;
            default:
                return false;
        }
    }

    public static List<(Vector2 A, Vector2 B)> Segments(SpaceShape shape, Vector2 center, float size, float rotation)
    {
        var list = new List<(Vector2 A, Vector2 B)>();

        switch (shape)
        {
            case SpaceShape.Line:
                list.Add((center + new Vector2(-size, 0f), center + new Vector2(size, 0f)));
                break;

            case SpaceShape.Triangle:
                {
                    Vector2 a = center + new Vector2(0f, -size);
                    Vector2 b = center + new Vector2(size * 0.87f, size * 0.5f);
                    Vector2 c = center + new Vector2(-size * 0.87f, size * 0.5f);
                    list.Add((a, b));
                    list.Add((b, c));
                    list.Add((c, a));
                    break;
                }

            case SpaceShape.Bridge:
            case SpaceShape.Plateau:
                {
                    float h = RectHalfHeight(shape, size);
                    Vector2 a = center + new Vector2(-size, -h);
                    Vector2 b = center + new Vector2(size, -h);
                    Vector2 c = center + new Vector2(size, h);
                    Vector2 d = center + new Vector2(-size, h);
                    list.Add((a, b));
                    list.Add((b, c));
                    list.Add((c, d));
                    list.Add((d, a));
                    break;
                }

            case SpaceShape.Step:
            case SpaceShape.Square:
                {
                    Vector2 a = center + new Vector2(-size, -size);
                    Vector2 b = center + new Vector2(size, -size);
                    Vector2 c = center + new Vector2(size, size);
                    Vector2 d = center + new Vector2(-size, size);
                    list.Add((a, b));
                    list.Add((b, c));
                    list.Add((c, d));
                    list.Add((d, a));
                    break;
                }

            case SpaceShape.Hexagon:
                {
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 a = center + new Vector2(0f, -size).RotatedBy(MathHelper.TwoPi * i / 6f + rotation);
                        Vector2 b = center + new Vector2(0f, -size).RotatedBy(MathHelper.TwoPi * (i + 1) / 6f + rotation);
                        list.Add((a, b));
                    }
                    break;
                }

            case SpaceShape.Circle:
                {
                    const int n = 40;
                    for (int i = 0; i < n; i++)
                    {
                        Vector2 a = center + new Vector2(size, 0f).RotatedBy(MathHelper.TwoPi * i / n + rotation);
                        Vector2 b = center + new Vector2(size, 0f).RotatedBy(MathHelper.TwoPi * (i + 1) / n + rotation);
                        list.Add((a, b));
                    }
                    break;
                }

            case SpaceShape.Diamond:
                {
                    Vector2 a = center + new Vector2(0f, -size);
                    Vector2 b = center + new Vector2(size * 0.7f, 0f);
                    Vector2 c = center + new Vector2(0f, size);
                    Vector2 d = center + new Vector2(-size * 0.7f, 0f);
                    list.Add((a, b));
                    list.Add((b, c));
                    list.Add((c, d));
                    list.Add((d, a));
                    break;
                }

            case SpaceShape.Star:
                {
                    Vector2[] p = new Vector2[5];
                    for (int i = 0; i < 5; i++)
                        p[i] = center + new Vector2(0f, -size).RotatedBy(MathHelper.TwoPi * i / 5f + rotation);
                    for (int i = 0; i < 5; i++)
                        list.Add((p[i], p[(i + 2) % 5]));
                    break;
                }
        }

        return list;
    }

    public static void DrawLine(Texture2D px, Vector2 a, Vector2 b, float width, Color color)
    {
        Vector2 d = b - a;
        float len = d.Length();
        if (len < 0.5f)
            return;

        Main.EntitySpriteDraw(px, a - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, d.ToRotation(),
            new Vector2(0f, 0.5f), new Vector2(len, width), SpriteEffects.None, 0);
    }

    public static void AddLine(List<DrawData> cache, Texture2D px, Vector2 a, Vector2 b, float width, Color color)
    {
        Vector2 d = b - a;
        float len = d.Length();
        if (len < 0.5f)
            return;

        cache.Add(new DrawData(px, a - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, d.ToRotation(),
            new Vector2(0f, 0.5f), new Vector2(len, width), SpriteEffects.None, 0f));
    }
}

public class SpaceShapeGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public bool HexSlowed;
    public bool RingBoosted;
}

public class SpaceShapePlayer : ModPlayer
{
    public bool Previewing;
    public int Selected = 1;
    public int Cooldown;

    public bool Enabled => DwarfRace.IsDwarf(Player);

    public static bool IsUnlocked(int index)
    {
        return index switch
        {
            0 => NPC.downedBoss1,
            1 => true,
            2 => NPC.downedBoss1 && NPC.downedBoss2,
            3 => NPC.downedBoss1 && NPC.downedQueenBee,
            4 => NPC.downedBoss3,
            5 => NPC.downedDeerclops,
            6 => Main.hardMode,
            7 => NPC.downedMechBossAny,
            8 => NPC.downedPlantBoss,
            9 => NPC.downedBoss1 && NPC.downedMoonlord,
            _ => false
        };
    }

    public static int UnlockedCount
    {
        get
        {
            int t = 0;
            for (int i = 0; i < SpaceShapeData.Count; i++)
            {
                if (IsUnlocked(i))
                    t++;
            }
            return t;
        }
    }

    public static int Tier => Math.Max(UnlockedCount - 1, 0);
    public static float TierScale => 1f + 0.07f * Tier;
    public static float TierLife => 1f + 0.1f * Tier;
    public static int MaxRectangles => NPC.downedPlantBoss ? 4 : Main.hardMode ? 3 : 2;

    public Vector2 GetPlacePosition()
    {
        Vector2 to = Main.MouseWorld - Player.Center;
        if (to.Length() > 520f)
            to = to.SafeNormalize(Vector2.UnitX) * 520f;
        return Player.Center + to;
    }

    private void EnsureSelection()
    {
        if (IsUnlocked(Selected))
            return;

        for (int i = 0; i < SpaceShapeData.Count; i++)
        {
            if (IsUnlocked(i))
            {
                Selected = i;
                return;
            }
        }
    }

    private void Cycle(int dir)
    {
        int n = Selected;
        for (int i = 0; i < SpaceShapeData.Count; i++)
        {
            n = (n + dir + SpaceShapeData.Count) % SpaceShapeData.Count;
            if (IsUnlocked(n))
            {
                Selected = n;
                SoundEngine.PlaySound(SoundID.MenuTick);
                return;
            }
        }
    }

    private void LimitRectangles()
    {
        int type = ModContent.ProjectileType<SpaceShapeProj>();
        int max = MaxRectangles;

        while (true)
        {
            int count = 0;
            Projectile oldest = null;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.type != type || p.owner != Player.whoAmI || !SpaceShapeData.IsRectangle((int)p.ai[0]))
                    continue;

                count++;
                if (oldest == null || p.localAI[0] > oldest.localAI[0])
                    oldest = p;
            }

            if (count < max || oldest == null)
                return;

            oldest.Kill();
        }
    }

    private void TryPlace()
    {
        EnsureSelection();
        if (!IsUnlocked(Selected))
            return;

        if (Cooldown > 0)
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            return;
        }

        if (SpaceShapeData.IsRectangle(Selected))
            LimitRectangles();

        Vector2 pos = GetPlacePosition();
        int dmg = (int)Player.GetTotalDamage(DamageClass.Generic).ApplyTo(SpaceShapeData.Damage[Selected]);

        Projectile.NewProjectile(Player.GetSource_Misc("SpaceShape"), pos, Vector2.Zero,
            ModContent.ProjectileType<SpaceShapeProj>(), dmg, 0f, Player.whoAmI, Selected);

        Cooldown = SpaceShapeData.Cooldown[Selected];
        SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.6f, Pitch = 0.3f }, pos);
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (!Enabled || Player.dead || UnlockedCount == 0)
        {
            Previewing = false;
            return;
        }

        ModKeybind key = VanillaKeybinds.RaceSpecial;

        if (key.Current)
        {
            EnsureSelection();
            Previewing = true;

            int delta = PlayerInput.ScrollWheelDelta;
            if (delta != 0)
            {
                Cycle(delta > 0 ? -1 : 1);
                PlayerInput.ScrollWheelDelta = 0;
            }
        }
        else
        {
            if (Previewing && key.JustReleased)
                TryPlace();
            Previewing = false;
        }
    }

    public override void PostUpdateEquips()
    {
        if (!Enabled)
            return;

        Player.wings = 0;
        Player.wingsLogic = 0;
        Player.wingTime = 0f;
        Player.wingTimeMax = 0;

        int type = ModContent.ProjectileType<SpaceShapeProj>();
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (!p.active || p.type != type || p.owner != Player.whoAmI || (int)p.ai[0] != (int)SpaceShape.Hexagon)
                continue;

            float r = SpaceShapeData.Size[(int)SpaceShape.Hexagon] * p.scale * 0.87f;
            if (Vector2.Distance(Player.Center, p.Center) < r)
            {
                Player.endurance += 0.15f;
                break;
            }
        }
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (Enabled)
            drawInfo.drawPlayer.wings = 0;
    }

    public override void PreUpdateMovement()
    {
        if (Player.dead || Player.controlDown || Player.velocity.Y < 0f || Player.grappling[0] >= 0)
            return;

        float bottom = Player.position.Y + Player.height;
        float next = bottom + Player.velocity.Y;
        int type = ModContent.ProjectileType<SpaceShapeProj>();

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (!p.active || p.type != type)
                continue;

            if (p.localAI[0] < 8f || p.localAI[1] - p.localAI[0] < 4f)
                continue;

            int si = Utils.Clamp((int)p.ai[0], 0, SpaceShapeData.Count - 1);
            float size = SpaceShapeData.Size[si] * p.scale;
            if (!SpaceShapeData.TryGetPlatform((SpaceShape)si, p.Center, size, out float y, out float half))
                continue;

            if (Player.Right.X < p.Center.X - half || Player.Left.X > p.Center.X + half)
                continue;

            if (bottom <= y + 2f && next >= y)
            {
                Player.position.Y = y - Player.height;
                Player.velocity.Y = 0f;
                Player.fallStart = (int)(Player.position.Y / 16f);
                Player.fallStart2 = Player.fallStart;
                break;
            }
        }
    }

    public override void PostUpdate()
    {
        if (Cooldown > 0)
            Cooldown--;
    }
}

public class SpaceShapeNoWingsItem : GlobalItem
{
    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
    {
        if (item.wingSlot > 0 && DwarfRace.IsDwarf(player))
            return false;

        return true;
    }
}

public class SpaceShapeProj : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_0";

    private readonly Dictionary<int, int> tossReady = new Dictionary<int, int>();
    private int bounceCooldown;
    private int starHealed;

    private int ShapeIndex => Utils.Clamp((int)Projectile.ai[0], 0, SpaceShapeData.Count - 1);
    private SpaceShape Shape => (SpaceShape)ShapeIndex;
    private float Size => SpaceShapeData.Size[ShapeIndex] * Projectile.scale;

    private float Fade
    {
        get
        {
            float age = Projectile.localAI[0];
            float life = Projectile.localAI[1];
            float fadeIn = MathHelper.Clamp(age / 14f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((life - age) / 20f, 0f, 1f);
            return Math.Min(fadeIn, fadeOut);
        }
    }

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 120;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.scale = SpaceShapePlayer.TierScale;
        Projectile.timeLeft = (int)(SpaceShapeData.Life[ShapeIndex] * SpaceShapePlayer.TierLife);
        Projectile.localAI[1] = Projectile.timeLeft;
        Projectile.friendly = SpaceShapeData.Damage[ShapeIndex] > 0;
    }

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanCutTiles() => false;

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = (p.X - b.X) * (a.Y - b.Y) - (a.X - b.X) * (p.Y - b.Y);
        float d2 = (p.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (p.Y - c.Y);
        float d3 = (p.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (p.Y - a.Y);
        bool neg = d1 < 0f || d2 < 0f || d3 < 0f;
        bool pos = d1 > 0f || d2 > 0f || d3 > 0f;
        return !(neg && pos);
    }

    private static bool CircleRect(Vector2 c, float r, Rectangle rect)
    {
        float x = MathHelper.Clamp(c.X, rect.Left, rect.Right);
        float y = MathHelper.Clamp(c.Y, rect.Top, rect.Bottom);
        return Vector2.DistanceSquared(c, new Vector2(x, y)) <= r * r;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float s = Size;
        switch (Shape)
        {
            case SpaceShape.Square:
                return new Rectangle((int)(Projectile.Center.X - s), (int)(Projectile.Center.Y - s), (int)(s * 2f), (int)(s * 2f)).Intersects(targetHitbox);
            case SpaceShape.Star:
                return CircleRect(Projectile.Center, s, targetHitbox);
            case SpaceShape.Diamond:
                return CircleRect(Projectile.Center, s * 0.5f, targetHitbox);
            default:
                return false;
        }
    }

    public override void AI()
    {
        Projectile.localAI[0]++;
        float age = Projectile.localAI[0];
        float s = Size;
        Player owner = Main.player[Projectile.owner];
        bool isOwner = Main.myPlayer == Projectile.owner;
        bool authority = Main.netMode != NetmodeID.MultiplayerClient;

        Projectile.rotation += 0.02f;
        Lighting.AddLight(Projectile.Center, SpaceShapeData.Colors[ShapeIndex].ToVector3() * 0.4f * Fade);

        if (bounceCooldown > 0)
            bounceCooldown--;

        switch (Shape)
        {
            case SpaceShape.Triangle:
                if (isOwner && age > 10f && bounceCooldown <= 0)
                {
                    Vector2 c = Projectile.Center;
                    Vector2 a = c + new Vector2(0f, -s);
                    Vector2 b = c + new Vector2(s * 0.87f, s * 0.5f);
                    Vector2 d = c + new Vector2(-s * 0.87f, s * 0.5f);

                    if (PointInTriangle(owner.Center, a, b, d))
                    {
                        owner.velocity.Y = -15f;
                        owner.fallStart = (int)(owner.position.Y / 16f);
                        owner.fallStart2 = owner.fallStart;
                        bounceCooldown = 45;
                        SoundEngine.PlaySound(SoundID.Item24 with { Volume = 0.6f, Pitch = 0.4f }, owner.Center);
                    }
                }
                break;

            case SpaceShape.Square:
                if (authority)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || npc.friendly || npc.boss || npc.dontTakeDamage)
                            continue;

                        if (Math.Abs(npc.Center.X - Projectile.Center.X) < s && Math.Abs(npc.Center.Y - Projectile.Center.Y) < s)
                            npc.velocity *= 0.86f;
                    }
                }
                break;

            case SpaceShape.Hexagon:
                if (authority)
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile p = Main.projectile[i];
                        if (!p.active || !p.hostile || p.friendly)
                            continue;

                        SpaceShapeGlobalProjectile g = p.GetGlobalProjectile<SpaceShapeGlobalProjectile>();
                        if (g.HexSlowed)
                            continue;

                        if (Vector2.Distance(p.Center, Projectile.Center) < s * 0.87f)
                        {
                            g.HexSlowed = true;
                            p.velocity *= 0.6f;
                            p.netUpdate = true;
                        }
                    }
                }
                break;

            case SpaceShape.Circle:
                if (isOwner)
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile p = Main.projectile[i];
                        if (!p.active || !p.friendly || p.owner != Projectile.owner || p.whoAmI == Projectile.whoAmI)
                            continue;
                        if (p.damage <= 0 || p.minion || p.sentry || p.type == Projectile.type)
                            continue;

                        SpaceShapeGlobalProjectile g = p.GetGlobalProjectile<SpaceShapeGlobalProjectile>();
                        if (g.RingBoosted)
                            continue;

                        float dist = Vector2.Distance(p.Center, Projectile.Center);
                        if (Math.Abs(dist - s) < 18f)
                        {
                            g.RingBoosted = true;
                            p.damage = (int)(p.damage * 1.15f);
                            p.netUpdate = true;
                            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.35f, Pitch = 0.5f }, p.Center);
                        }
                    }
                }
                break;

            case SpaceShape.Diamond:
                if (authority)
                {
                    float reach = s * 1.8f;
                    float core = s * 0.5f;

                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || npc.friendly || npc.boss || npc.dontTakeDamage || npc.knockBackResist <= 0f)
                            continue;

                        Vector2 to = Projectile.Center - npc.Center;
                        float dist = to.Length();
                        if (dist > reach)
                            continue;

                        if (dist <= core)
                        {
                            if (!tossReady.TryGetValue(i, out int next) || age >= next)
                            {
                                npc.velocity.X *= 0.3f;
                                npc.velocity.Y = -12f * (0.4f + 0.6f * npc.knockBackResist);
                                npc.netUpdate = true;
                                tossReady[i] = (int)age + 35;
                            }
                        }
                        else
                        {
                            npc.velocity += to.SafeNormalize(Vector2.Zero) * 0.45f * npc.knockBackResist;
                            if (npc.velocity.Length() > 8f)
                                npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 8f;
                            if (age % 4f == 0f)
                                npc.netUpdate = true;
                        }
                    }
                }
                break;

            case SpaceShape.Star:
                if (isOwner && starHealed < 50 && age % 45f == 0f && Vector2.Distance(owner.Center, Projectile.Center) < s)
                {
                    int amount = Math.Min(10, 50 - starHealed);
                    owner.Heal(amount);
                    starHealed += amount;
                }
                break;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D px = TextureAssets.MagicPixel.Value;
        Rectangle src = new Rectangle(0, 0, 1, 1);
        float fade = Fade;
        float age = Projectile.localAI[0];
        float s = Size;
        Color c = SpaceShapeData.Colors[ShapeIndex];
        Vector2 center = Projectile.Center;
        Vector2 half = new Vector2(0.5f, 0.5f);

        var segs = SpaceShapeData.Segments(Shape, center, s, Projectile.rotation);
        float build = MathHelper.Clamp(age / 16f, 0f, 1f);
        int n = segs.Count;

        Color fill = new Color(c.R, c.G, c.B, 0) * (0.07f * fade);
        switch (Shape)
        {
            case SpaceShape.Square:
            case SpaceShape.Step:
                Main.EntitySpriteDraw(px, center - Main.screenPosition, src, fill, 0f, half, new Vector2(s * 2f, s * 2f), SpriteEffects.None, 0);
                break;
            case SpaceShape.Bridge:
            case SpaceShape.Plateau:
                Main.EntitySpriteDraw(px, center - Main.screenPosition, src, fill, 0f, half,
                    new Vector2(s * 2f, SpaceShapeData.RectHalfHeight(Shape, s) * 2f), SpriteEffects.None, 0);
                break;
        }

        for (int i = 0; i < n; i++)
        {
            float t = MathHelper.Clamp(build * n - i, 0f, 1f);
            if (t <= 0f)
                continue;

            Vector2 a = segs[i].A;
            Vector2 b = Vector2.Lerp(segs[i].A, segs[i].B, t);

            SpaceShapeData.DrawLine(px, a, b, 8f, new Color(c.R, c.G, c.B, 0) * (0.16f * fade));
            SpaceShapeData.DrawLine(px, a, b, 4f, new Color(c.R, c.G, c.B, 0) * (0.45f * fade));
            SpaceShapeData.DrawLine(px, a, b, 1.8f, new Color(255, 255, 255, 0) * (0.9f * fade));
        }

        if (SpaceShapeData.TryGetPlatform(Shape, center, s, out float py, out float hw))
        {
            float k = MathHelper.Clamp(build * 1.5f, 0f, 1f);
            SpaceShapeData.DrawLine(px, new Vector2(center.X - hw * k, py), new Vector2(center.X + hw * k, py), 3f,
                new Color(255, 255, 255, 0) * (0.7f * fade));
        }

        if (Shape == SpaceShape.Star || Shape == SpaceShape.Circle || Shape == SpaceShape.Hexagon || Shape == SpaceShape.Diamond)
        {
            float pulse = age % 45f / 45f;
            var ring = SpaceShapeData.Segments(SpaceShape.Circle, center, s * pulse, 0f);
            foreach (var seg in ring)
                SpaceShapeData.DrawLine(px, seg.A, seg.B, 2f, new Color(c.R, c.G, c.B, 0) * ((1f - pulse) * 0.45f * fade));
        }

        return false;
    }
}

public class SpaceShapePreviewLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        Player p = drawInfo.drawPlayer;
        return p.whoAmI == Main.myPlayer && p.GetModPlayer<SpaceShapePlayer>().Previewing;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        SpaceShapePlayer sp = player.GetModPlayer<SpaceShapePlayer>();
        List<DrawData> cache = drawInfo.DrawDataCache;

        Texture2D px = TextureAssets.MagicPixel.Value;
        Rectangle src = new Rectangle(0, 0, 1, 1);
        Vector2 half = new Vector2(0.5f, 0.5f);
        float time = Main.GlobalTimeWrappedHourly;

        int index = sp.Selected;
        if (!SpaceShapePlayer.IsUnlocked(index))
            return;

        Color c = SpaceShapeData.Colors[index];
        Vector2 pos = sp.GetPlacePosition();
        float size = SpaceShapeData.Size[index] * SpaceShapePlayer.TierScale;
        float pulse = 0.5f + 0.5f * (float)Math.Sin(time * 6f);
        float alpha = sp.Cooldown > 0 ? 0.22f : 0.5f + 0.3f * pulse;

        SpaceShapeData.AddLine(cache, px, player.Center, pos, 1.5f, new Color(c.R, c.G, c.B, 0) * 0.14f);

        foreach (var seg in SpaceShapeData.Segments((SpaceShape)index, pos, size, time * 0.4f))
        {
            SpaceShapeData.AddLine(cache, px, seg.A, seg.B, 5f, new Color(c.R, c.G, c.B, 0) * (alpha * 0.3f));
            SpaceShapeData.AddLine(cache, px, seg.A, seg.B, 1.6f, new Color(255, 255, 255, 0) * alpha);
        }

        if (SpaceShapeData.TryGetPlatform((SpaceShape)index, pos, size, out float py, out float hw))
        {
            SpaceShapeData.AddLine(cache, px, new Vector2(pos.X - hw, py), new Vector2(pos.X + hw, py), 2.5f,
                new Color(255, 255, 255, 0) * (alpha * 0.8f));
        }

        List<int> unlocked = new List<int>();
        for (int i = 0; i < SpaceShapeData.Count; i++)
        {
            if (SpaceShapePlayer.IsUnlocked(i))
                unlocked.Add(i);
        }

        int n = unlocked.Count;
        float spacing = 28f;
        Vector2 stripOrigin = Main.MouseWorld + new Vector2(0f, 40f);

        for (int k = 0; k < n; k++)
        {
            int idx = unlocked[k];
            bool sel = idx == index;
            Color col = SpaceShapeData.Colors[idx];
            Vector2 cell = stripOrigin + new Vector2((k - (n - 1) / 2f) * spacing, sel ? -3f : 0f);
            float mini = SpaceShapeData.MiniSize[idx] * (sel ? 1.2f : 0.9f);
            float iconAlpha = sel ? (sp.Cooldown > 0 ? 0.55f : 1f) : 0.4f;

            cache.Add(new DrawData(px, cell - Main.screenPosition, src, new Color(8, 14, 26) * (sel ? 0.6f : 0.35f), 0f, half,
                new Vector2(24f, 24f), SpriteEffects.None, 0f));

            if (sel)
            {
                Color frame = new Color(col.R, col.G, col.B, 0) * 0.9f;
                SpaceShapeData.AddLine(cache, px, cell + new Vector2(-12f, -12f), cell + new Vector2(12f, -12f), 1.5f, frame);
                SpaceShapeData.AddLine(cache, px, cell + new Vector2(-12f, 12f), cell + new Vector2(12f, 12f), 1.5f, frame);
                SpaceShapeData.AddLine(cache, px, cell + new Vector2(-12f, -12f), cell + new Vector2(-12f, 12f), 1.5f, frame);
                SpaceShapeData.AddLine(cache, px, cell + new Vector2(12f, -12f), cell + new Vector2(12f, 12f), 1.5f, frame);
            }

            foreach (var seg in SpaceShapeData.Segments((SpaceShape)idx, cell, mini, sel ? time * 0.8f : 0f))
            {
                SpaceShapeData.AddLine(cache, px, seg.A, seg.B, 3f, new Color(col.R, col.G, col.B, 0) * (iconAlpha * 0.3f));
                SpaceShapeData.AddLine(cache, px, seg.A, seg.B, 1.2f, new Color(255, 255, 255, 0) * iconAlpha);
            }
        }
    }
}