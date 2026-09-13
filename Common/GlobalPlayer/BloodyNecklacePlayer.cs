using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Waybound.Common.ModSystems;
using Waybound.Common.Utils;

namespace Waybound.Common.GlobalPlayer;

public class BloodyNecklacePlayer : ModPlayer {
    public int ActiveMode { get => _activeMode; set => _activeMode = equipped ? (value > 4 ? 0 : value) : 0; }
    public int CursedHP {
        get {
            int currentHP = Player.ConsumedLifeCrystals + 5;
            float scale = UI.GetProgress(currentHP, Player.LifeCrystalMax + currentHP < 5 ? 5 : 0);
            int heartScale = (int)((Player.statLifeMax2 * scale) / (currentHP));
            return ActiveMode > 0 ? ((Player.statLifeMax2 - _maxHP) / (heartScale)) : -1;
        }
    }

    int _activeMode = 0;
    int _maxHP = 0;

    bool _maxDonation = false;
    public bool equipped = false;

    public override void LoadData(TagCompound tag) {
        _activeMode = tag.GetInt($"{Waybound.ModName}: Active mode");
        _maxDonation = tag.GetBool($"{Waybound.ModName}: Active Buff");
    }
    public override void SaveData(TagCompound tag) {
        tag[$"{Waybound.ModName}: Active mode"] = _activeMode;
        tag[$"{Waybound.ModName}: Active Buff"] = _maxDonation;
    }
    public override void ResetEffects() => equipped = false;
    public override void PostUpdate() {
        if (ActiveMode == 0) { return; }
        _maxHP = (int)(Player.statLifeMax2 * GetLevel(1f, .8f, .6f, .4f, .2f));
        if (Player.statLife > _maxHP) { Player.statLife = _maxHP; };
        GetLevel(null, () => { Player.GetDamage(DamageClass.Generic) += 0.1f; }, null, () => { Player.GetDamage(DamageClass.Generic) += 0.2f; }, () => { Bonus(); });
    }
    public override void UpdateLifeRegen() => Player.lifeRegen += ActiveMode == 2 ? 10 : 0;
    public override void ProcessTriggers(TriggersSet triggersSet) {
        if (VanillaKeybinds.AccBonusActivation.JustPressed && equipped && !Player.dead) {
            if (ActiveMode < 4) { ActiveMode++; }
            if (_maxDonation) {
                Player.GetModPlayer<GlobalPlayerData>().ActiveTextData.Add(new(Loc.GetUI("BloodyNecklacePlayer.Max")) { hasAlpha = false, multMaxTime = 60 });
                CombatText.NewText(Player.getRect(), Color.DarkRed, Loc.GetUI("BloodyNecklacePlayer.Max"));
                SoundEngine.PlaySound(SoundID.MaxMana, Player.position);
                return;
            };

            string levelText = Loc.GetUI("BloodyNecklacePlayer.CurrentMode.Text") + " ";
            string text = GetLevel(levelText + Loc.GetUI("BloodyNecklacePlayer.CurrentMode.0") + BonusText(ActiveMode), levelText + ActiveMode + BonusText(ActiveMode), levelText + ActiveMode + BonusText(ActiveMode), levelText + ActiveMode + BonusText(ActiveMode), levelText + ActiveMode + BonusText(ActiveMode));
            Player.GetModPlayer<GlobalPlayerData>().ActiveTextData.Add(new(text) { hasAlpha = false, multMaxTime = 60 });
            CombatText.NewText(Player.getRect(), Color.DarkRed, text);
            SoundEngine.PlaySound(Resources.Audio.Get("BloodyNecklace"), Player.position);
            if (ActiveMode == 4) { _maxDonation = true; };
            for (int i = 0; i < 17 + 3 * ActiveMode; i++) { Dust.NewDust(Player.position, Player.width, Player.height, DustID.LifeDrain, 0f, 0f, 255, default, Main.rand.Next(20, 26) * 0.1f); };
            static string BonusText(int lvl) => "\n" + Loc.GetUI("BloodyNecklacePlayer.Bonus.Text") + " " + Loc.GetUI($"BloodyNecklacePlayer.Bonus.{lvl}");
        }
    }
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
        ActiveMode = 0;
        _maxDonation = false;
    }
    T GetLevel<T>(T value0, T value1, T value2, T value3, T value4) {
        T value = value0;
        if (ActiveMode == 0) { value = value0; };
        if (ActiveMode == 1) { value = value1; };
        if (ActiveMode == 2) { value = value2; };
        if (ActiveMode == 3) { value = value3; };
        if (ActiveMode == 4) { value = value4; };
        if (value is Action action) { action?.Invoke(); };
        return value;
    }
    void Bonus() {

    }
};