using System;
using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.WUtils;
using Waybound.Particles;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Armor.Melee.DespairArmor;

[AutoloadEquip(EquipType.Head)]
public sealed class DespairHelmet : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 20;
        Item.rare = ItemRarityID.Pink;
        Item.defense = 17;
        Item.value = Item.sellPrice(0, 3, 4, 50);
    }
    public override void UpdateEquip(Player player)
    {
        player.GetAttackSpeed(DamageClass.Melee) += 0.08f;
        player.GetCritChance(DamageClass.Melee) += 10f;
    }
    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ItemType<DespairBreastplate>() && legs.type == ItemType<DespairLeggings>() && head.type == Type;
    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = Loc.GetTips("Armor.DespairSetBonus");
        player.GetDamage(DamageClass.Melee) += 0.04f;
        player.GetModPlayer<DespairPlayer>().setActive = true;
    }
}


// Crystal shards which deal damage to enemies on hit and heal the player

public sealed class DespairPlayer : ModPlayer
{
    public const int MaxShards = 4;
    public const float DamageReduction = 0.20f;     
    public const float HealPercent = 0.03f;        
    public const float NR = 170f; //idk why am i calling every explosion a nova, RADIUS
    public const float NBD = 37f;     //DAMAGE
    public const float MaxMissingHpNovaBonus = 0.5f; 
    public const int HitsPerShard = 6;              
    public const int HitGainCooldown = 20;         
    public const int PassiveRegenTicks = 60 * 12;   
    private const float OrbitRadius = 46f;
    private const float OrbitSquash = 0.5f;         // Something like 3d orbital that i did for crystal but easier way

    public bool setActive;
    public int shards;

    private bool wasActive;
    private bool absorbing;
    private float orbitAngle;
    private float pulse;
    private int hitProgress;
    private int regenTimer;
    private int hitCooldown;

    public override void ResetEffects()
    {
        setActive = false;
        absorbing = false;
    }

    public override void UpdateDead()
    {
        shards = 0;
        wasActive = false;
        hitProgress = 0;
        regenTimer = 0;
        hitCooldown = 0;
        pulse = 0f;
    }

    public override void PostUpdateEquips()
    {
        if (hitCooldown > 0) hitCooldown--;

        if (!setActive)
        {
            shards = 0;
            wasActive = false;
            hitProgress = 0;
            regenTimer = 0;
            return;
        }

        if (!wasActive)
        {
            wasActive = true;
            shards = MaxShards;
            hitProgress = 0;
            regenTimer = 0;
        }

        float missing = MissingHp();

        orbitAngle = (orbitAngle + 0.035f + 0.04f * missing) % MathHelper.TwoPi;
        pulse = Math.Max(0f, pulse - 0.06f);

        if (shards < MaxShards)
        {
            if (++regenTimer >= PassiveRegenTicks)
                RestoreShard();
        }
        else
        {
            regenTimer = 0;
        }

        DrawShards(missing);
    }
    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (!setActive || shards <= 0) return;
        modifiers.FinalDamage *= 1f - DamageReduction;
        absorbing = true;
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        if (!absorbing) return;
        absorbing = false;
        if (!setActive || shards <= 0) return;
        ShatterShard();
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (item.CountsAsClass(DamageClass.Melee)) RegisterHit(target);
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (proj.CountsAsClass(DamageClass.Melee)) RegisterHit(target);
    }

    private static bool IsValidTarget(NPC target) => !target.friendly && target.lifeMax > 5 && !target.immortal;

    private float MissingHp() => MathHelper.Clamp(1f - Player.statLife / (float)Player.statLifeMax2, 0f, 1f);

    private void RegisterHit(NPC target)
    {
        if (!setActive || !IsValidTarget(target)) return;

        pulse = 1f; 

        if (shards >= MaxShards || hitCooldown > 0) return;
        hitCooldown = HitGainCooldown;

        if (++hitProgress >= HitsPerShard)
        {
            hitProgress = 0;
            RestoreShard();
        }
    }

    private void RestoreShard()
    {
        if (shards >= MaxShards) return;

        shards++;
        regenTimer = 0;
        Vector2 slot = GetSlot(shards - 1, out _, out _);
        Color color = Tint(MissingHp());
        const int life = 18;
        for (int i = 0; i < 12; i++)
        {
            Vector2 from = slot + Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(30f, 56f);
            float size = Main.rand.NextFloat(6f, 11f);
            Spawn(from, (slot - from) / life, Main.rand.NextFloat(MathHelper.TwoPi),
                size, size * 1.1f, color * Main.rand.NextFloat(0.7f, 1f), life);
        }

        SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.5f, Pitch = 0.3f }, Player.Center);
    }

    private void ShatterShard()
    {
        float missing = MissingHp();
        Vector2 origin = GetSlot(shards - 1, out _, out _);
        shards--;
        regenTimer = 0;
        Color color = Tint(missing);
        for (int i = 0; i < 18; i++)
        {
            Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
            float size = Main.rand.NextFloat(7f, 16f);
            Spawn(origin, dir * Main.rand.NextFloat(2f, 6f), Main.rand.NextFloat(MathHelper.TwoPi),
                size, size * Main.rand.NextFloat(0.75f, 1.05f), color * Main.rand.NextFloat(0.85f, 1.15f), Main.rand.Next(22, 38));
        }

        const int ringCount = 28;
        const int ringLife = 28;
        for (int i = 0; i < ringCount; i++)
        {
            float a = MathHelper.TwoPi * i / ringCount + Main.rand.NextFloat(-0.05f, 0.05f);
            Vector2 dir = a.ToRotationVector2();
            float size = Main.rand.NextFloat(10f, 18f);
            Spawn(Player.Center + dir * 10f, dir * (NR / ringLife), a + MathHelper.PiOver2,
                size, size * 1.2f, Color.Lerp(color, new Color(255, 255, 255, 0), 0.25f), ringLife);
        }

        for (int i = 0; i < 10; i++)
        {
            Vector2 dir = Main.rand.NextVector2Circular(1f, 1f);
            float size = Main.rand.NextFloat(5f, 9f);
            Spawn(Player.Center + dir * Main.rand.NextFloat(10f, 40f), dir * 0.6f + new Vector2(0f, 0.4f),
                Main.rand.NextFloat(MathHelper.TwoPi), size, size, color * 0.8f, Main.rand.Next(40, 60));
        }

        SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.8f, PitchVariance = 0.2f }, Player.Center);

        if (Player.statLife > 0)
            Player.Heal(Math.Max(5, (int)(Player.statLifeMax2 * HealPercent)));

        if (Main.myPlayer != Player.whoAmI) return;

        int damage = (int)(Player.GetTotalDamage(DamageClass.Melee).ApplyTo(NBD) * (1f + missing * MaxMissingHpNovaBonus));
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy()) continue;
            if (Vector2.Distance(npc.Center, Player.Center) > NR + npc.width * 0.5f) continue;

            int dir = npc.Center.X >= Player.Center.X ? 1 : -1;
            NPC.HitInfo hit = npc.CalculateHitInfo(damage, dir, false, 6f, DamageClass.Melee, true, Player.luck);
            npc.StrikeNPC(hit);
            if (Main.netMode != NetmodeID.SinglePlayer)
                NetMessage.SendStrikeNPC(npc, hit);

            for (int i = 0; i < 6; i++)
            {
                float size = Main.rand.NextFloat(6f, 12f);
                Spawn(npc.Center, Main.rand.NextVector2Circular(3f, 3f), Main.rand.NextFloat(MathHelper.TwoPi),
                    size, size, color, Main.rand.Next(16, 28));
            }
        }
    }
    private Vector2 GetSlot(int index, out Vector2 tangent, out float front)
    {
        float ang = orbitAngle + index * MathHelper.TwoPi / MaxShards;
        float radius = OrbitRadius + pulse * 12f;
        float cos = (float)Math.Cos(ang);
        float sin = (float)Math.Sin(ang);

        tangent = new Vector2(-sin * radius, cos * radius * OrbitSquash);
        front = 0.5f + 0.5f * sin;
        return Player.Center + new Vector2(cos * radius, sin * radius * OrbitSquash - 6f);
    }

    private void DrawShards(float missing)
    {
        if (Main.netMode == NetmodeID.Server) return;

        Color color = Tint(missing);

        for (int i = 0; i < shards; i++)
        {
            Vector2 pos = GetSlot(i, out Vector2 tangent, out float front);
            Vector2 dir = tangent.SafeNormalize(Vector2.UnitX);
            float size = MathHelper.Lerp(11f, 17f, front);
            Spawn(pos, Vector2.Zero, dir.ToRotation() + MathHelper.PiOver2,
                size, size * 1.15f, color * MathHelper.Lerp(0.6f, 1f, front), 4);
            if (Main.GameUpdateCount % 2 == 0)
            {
                float s = Main.rand.NextFloat(5f, 8f) * MathHelper.Lerp(0.7f, 1f, front);
                Spawn(pos, -dir * 0.6f + Main.rand.NextVector2Circular(0.25f, 0.25f), Main.rand.NextFloat(MathHelper.TwoPi),
                    s, s, color * 0.55f, Main.rand.Next(14, 22));
            }
        }
        if (Main.rand.NextBool(Math.Max(8, 30 - (int)(missing * 20f))))
        {
            float s = Main.rand.NextFloat(4f, 7f);
            Spawn(Player.Center + Main.rand.NextVector2Circular(22f, 30f), new Vector2(0f, -0.5f),
                Main.rand.NextFloat(MathHelper.TwoPi), s, s, color * 0.7f, Main.rand.Next(30, 45));
        }
    }
    private static Color Tint(float missing)
    {
        Color lilac = new Color(200, 150, 255, 0);
        Color rose = new Color(255, 105, 150, 0);
        return missing < 0.5f
            ? Color.Lerp(lilac, lilac, missing * 2f)
            : Color.Lerp(lilac, rose, (missing - 0.5f) * 2f);
    }

    private static void Spawn(Vector2 pos, Vector2 vel, float rotation, float w, float h, Color color, int life)
    {
        if (Main.netMode == NetmodeID.Server) return;
        ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
            pos.ToNumerics(),
            vel.ToNumerics(),
            rotation,
            new SystemVector2(w, h),
            color,
            life
        ));
    }
}