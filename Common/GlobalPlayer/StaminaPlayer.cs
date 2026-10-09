using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Waybound.Common.GlobalPlayer;

public class StaminaPlayer : ModPlayer {
    public const int BASE_STAMINA_COUNT = 40;
    public const int BASE_LAST_ATACK_TIME = 180;
    public const int BASE_CD_TIME = 300;

    public int StaminaCount => Math.Abs(_staminaCount - MaxStatminaCount);
    public int MaxStatminaCount => BASE_STAMINA_COUNT + stainaMult;
    public int LastAtackTime => _lastAtackTime;
    public int MaxLastAtackTime => BASE_LAST_ATACK_TIME + lastAtackMult;
    public int CdTimeCount => _cdTimeCount;

    public float BarAlpha => _alpha;
    public float Intensity {
        get {
            if (MaxStatminaCount > 40) {
                int count = MaxStatminaCount - 40;
                if (count < 10) return 1.25f;
                float scale = 0;
                for (int i = 0; i < count / 10; i++) scale += 0.25f;
                return 1.75f + scale;
            }
            return 1.75f;
        }
    }
    public float BarGlowTime => (float)(Math.Sin(Main.GlobalTimeWrappedHourly * Intensity) * 0.5 + 0.5);
    public float StaminaPercent => MaxStatminaCount <= 0 ? 0f : (float)StaminaCount / MaxStatminaCount;

    public bool IsOverheating => _cdTimeCount > 0;
    public bool IsInPulseWindow => IsOverheating && BarGlowTime >= 0.95f;

    public bool Active => (_staminaCount == MaxStatminaCount || CdTimeCount != 0) && _alpha == 0;
    public bool Active2 => canUseItem;

    public int stainaMult = 0;
    public int lastAtackMult = 0;
    public int reduceTimeMult = 1;
    public int cdTimeMult = 0;
    internal int mapScaleY = 0;

    int _staminaCount = 0;
    int _lastAtackTime = 0;
    int _cdTimeCount = 0;
    float _alpha = 0f;
    bool canUseItem = true;
    bool _tick = false;

    public int GetEmpoweredShotCount() {
        float ratio = MathHelper.Clamp((MaxStatminaCount - 40) / 40f, 0f, 1f);
        if (ratio > 0.75f) return Main.rand.Next(2, 4);
        if (ratio > 0.35f) return Main.rand.Next(1, 3);
        return 1;
    }
    public void UseItem(int count = 1, bool atackTimer = true) {
        int count2 = _staminaCount + count;
        if (count2 >= MaxStatminaCount) _staminaCount = MaxStatminaCount;
        else _staminaCount = count2;

        if (atackTimer && !Active) SetAtackTime();
    }
    public void SetAtackTime() => _lastAtackTime = MaxLastAtackTime;

    public override void ResetEffects() {
        stainaMult = 0;
        lastAtackMult = 0;
        reduceTimeMult = 1;
        cdTimeMult = 0;
    }
    public override void PostUpdate() {
        if (ModCompat.DragonLens.GetGoodMode(Player)) { _staminaCount = 0; };
        if (StaminaCount >= MaxStatminaCount) { canUseItem = true; };
        if (IsOverheating && BarGlowTime >= 0.80f && !_tick) {
            SoundEngine.PlaySound(SoundID.AbigailAttack);
            _tick = true;
        }
        else if (BarGlowTime < 0.80f) { _tick = false; };
        if (LastAtackTime > 0) { _lastAtackTime = Utils.Clamp(_lastAtackTime - reduceTimeMult, 0, MaxLastAtackTime); };
        if (CdTimeCount == 0 && _staminaCount >= MaxStatminaCount) {
            _lastAtackTime = 0;
            _cdTimeCount = BASE_CD_TIME + cdTimeMult;
        };
        if (CdTimeCount > 0 && LastAtackTime <= 0) {
            _cdTimeCount--;
            _alpha = MathHelper.Lerp(_alpha, 1f, 0.25f);
        };
        if (CdTimeCount <= 0 && LastAtackTime <= 0) {
            if (!Active) _staminaCount = Utils.Clamp(_staminaCount - 1 * ((int)(MaxStatminaCount * 0.01f) + 1), 0, MaxStatminaCount);
            _alpha = MathHelper.Lerp(_alpha, 0f, 0.25f);
        };
        if (canUseItem && _staminaCount >= MaxStatminaCount) { canUseItem = false; };
    }
    public override void UpdateDead() {
        _staminaCount = 0;
        _lastAtackTime = 0;
        _cdTimeCount = 0;
        _alpha = 0f;
    }
}