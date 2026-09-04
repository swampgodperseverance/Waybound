using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Waybound.Common.Utils;
using Waybound.Content.Buffs.Misc;

namespace Waybound.Common.GlobalPlayer;

public class ThunderSigilPlayer : ModPlayer {
    public readonly int NEEDTIME = 300;

    public int WorkTime { get => _workTime; set => _workTime = equipped ? (value > NEEDTIME ? NEEDTIME : value < 0 ? 0 : value) : 0; }
    int _workTime = 0;
    public int npcIndex = -1;
    public int _currentIndex = -1;

    public float BarAlpha { get; private set; } = 0f;
    public float OutLineAlpha { get; private set; } = 0f;
    float _plaza = 0f;

    public bool HoverNPC => npcIndex != -1 && npcIndex == _currentIndex;
    public bool activeEffect = false;
    public bool visualOnly = false;
    public bool equipped = false;
    bool _tick = false;

    public override void ResetEffects() => equipped = false;
    public override void PostUpdate() {
        if (equipped) { Player.AddBuff(BuffType<ThunderSigilBuff>(), 1); }
        if (!equipped && activeEffect) { activeEffect = false; };
        if (!equipped) {
            UpdateAlpha(true);
            return; 
        };
        if (WorkTime >= NEEDTIME) { activeEffect = true; };
        if (_currentIndex != npcIndex) { _currentIndex = npcIndex; };
        if (!activeEffect) {
            if (HoverNPC && !Player.mouseInterface) {
                if (!visualOnly) { WorkTime++; }
                else { WorkTime -= 2; };
            } else if (Player.mouseInterface) {
                WorkTime -= 2;
                if (WorkTime == 0) { UpdateAlpha(true); };
            };
        }
        if (activeEffect) {
            if (!_tick) { 
                SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);
                _tick = true;
            };
            UpdateOutLineAlpha(false);
        }
        else { UpdateOutLineAlpha(true); };

        if (BarAlpha > 0) {
            Vector2 worldPos = Main.MouseWorld.X(17 + Resources.Textures.Extaras[0].Value.Width / 2).Y(21 + (Resources.Textures.Extaras[0].Value.Height / 2));
            _plaza = OutLineAlpha > 0 ? MathF.Sin(Main.GlobalTimeWrappedHourly * 3f) * 0.5f + 0.5f : 0;
            float scale = activeEffect ? BarAlpha + _plaza / 2 : UI.GetProgress(WorkTime, NEEDTIME) + _plaza / 2;
            Lighting.AddLight(worldPos, new Vector3(1.0f * scale, 0.75f * scale, 0.2f * scale));
        }

        float scale2 = activeEffect ? 0.2f : 0;
        Lighting.AddLight(Player.Center, new Vector3(0.8f + scale2, 0.75f + scale2, 0.2f + scale2));
    }
    public void UpdateAlpha(bool negativ) {
        if (negativ) {
            BarAlpha = MathHelper.Clamp(BarAlpha - 0.02f, 0f, 1f);
            if (BarAlpha < 0.01f) { BarAlpha = 0f; };
        }
        else { BarAlpha = MathHelper.Clamp(BarAlpha + 0.02f, 0f, 1f); };
    }
    public void UpdateOutLineAlpha(bool negativ) {
        if (negativ) {
            OutLineAlpha = MathHelper.Clamp(OutLineAlpha - 0.02f, 0f, 1f);
            if (OutLineAlpha < 0.01f) { OutLineAlpha = 0f; };
        }
        else { OutLineAlpha = MathHelper.Clamp(OutLineAlpha + 0.02f, 0f, 1f); };
    }
    public override bool FreeDodge(Player.HurtInfo info) {
        if (activeEffect) {
            Player.immune = true;
            Player.immuneTime = 20;
            WorkTime = 0;
            activeEffect = false;
            _tick = false;
            OutLineAlpha = 0f;
            for (int i = 0; i < 17; i++) {
                int index = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Pixie, 0f, 0f, 255, default, Main.rand.Next(20, 26) * 0.1f);
                Main.dust[index].noLight = true;
                Main.dust[index].velocity *= 0.5f;
            };
            return true;
        }
        else { return base.FreeDodge(info); }
    }
};