using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Waybound.Common.WUtils;
using Waybound.Content.Race;
using Waybound.Core;

namespace Waybound.UIs.Elements;

public class RaceSlot(PlayerCreationData data) {
    static RaceInfo _clon = null;

    readonly List<(string text, Color color)> _alltext = [];
    List<(string text, Color color)> _visibilityText = [];

    readonly float[] _scale = new float[3];
    int _removeIndex = -1;

    readonly bool[] _tick = new bool[3];
    bool _hover = false;
    bool _init = false;

    public void DrawSlot(SpriteBatch sb, Asset<Texture2D> raceMicroIcon, RaceInfo race, Vector2 pos, float alpha, string text, bool big = false) {
        Asset<Texture2D>[] asset = Resources.Textures.RaceElements;
        int slot = big ? 1 : 3;
        UI.DrawTexture(sb, asset[slot].Value, pos, scale: _scale[0], color: Color.White * alpha);
        if (_hover) { UI.DrawTexture(sb, asset[slot + 1].Value, pos, scale: _scale[0], color: Color.White * alpha); };
        UI.DrawTexture(sb, raceMicroIcon.Value, pos, scale: _scale[0] + (big ? 0.6f : 0), color: Color.White * alpha);
        if (UI.Hover(pos, asset[slot].Value)) {
            if (!_tick[0]) {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _tick[0] = true;
            };
            _hover = true;
            if (!big) { _scale[0] = MathHelper.Clamp(_scale[0] + 0.01f, 1f, 1.13f); };
            if (_scale[0] < 0.01f) { _scale[0] = 0f; };
            if (UI.LeftClick() && !big) {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                data.race = race;
                data.asset = raceMicroIcon;
            };
            Main.instance.MouseText(text);
        } else {
            if (!big) { _scale[0] = MathHelper.Clamp(_scale[0] - 0.01f, 1f, 1.13f); };
            if (_scale[0] < 1f) { _scale[0] = 1f; };
            _tick[0] = false;
            _hover = false;
        };
    }
    public void DrawText(SpriteBatch sB, RaceInfo race, Vector2 pos, float alpha) {
        string info = race.Info;
        string pozitiv = race.Pozitiv;
        string negativ = race.Negativ;

        if (!_init) { Init(race, info, pozitiv, negativ); _init = true; }
        else if (_clon.RaceName != race.RaceName) { Init(race, info, pozitiv, negativ); };

        int maxCount = _visibilityText.Count;
        int count = Math.Min(5, _alltext.Count);
        Vector2 drawPos = new(pos.X + 340, pos.Y + 10);

        float lineHeight = FontAssets.MouseText.Value.MeasureString("A").Y * 0.80f;

        for (int i = 0; i < count; i++) {
            var (text, color) = _visibilityText[i];
            UI.DrawText(sB, text, pos, color: color * alpha, scale: new(.80f));
            pos.Y += lineHeight;
        };

        DrawArrow(sB, drawPos, Loc.GetUI("PlayerRaceMenu.PL"), _removeIndex != -1, 1, () => {
            _visibilityText.Insert(0, _alltext[_removeIndex]);
            _removeIndex--;
        }, alpha);
        DrawArrow(sB, drawPos.Y(80), Loc.GetUI("PlayerRaceMenu.NL"), _visibilityText.Count > 0 && maxCount > 5, 2, () => {
            _visibilityText.RemoveAt(0);
            _removeIndex++;
        }, alpha, true);
    }
    void DrawArrow(SpriteBatch sB, Vector2 drawPos, string text, bool active, int index, Action action, float alpha, bool flip = false) {
        Texture2D texture = Resources.Textures.RaceElements[16].Value;
        Rectangle frame = texture.Frame(1, 2, 0, active ? 0 : 1);
        UI.DrawTexture(sB, texture, drawPos, sourceRectangle: frame, effects: flip ? SpriteEffects.FlipVertically : SpriteEffects.None, scale: _scale[index], color: Color.White * alpha);
        if (UI.Hover(drawPos, frame)) {
            if (active) { _scale[index] = MathHelper.Clamp(_scale[index] + 0.01f, 1f, 1.13f); };
            texture = Resources.Textures.RaceElements[19].Value;
            frame = texture.Frame(1, 2, 0, active ? 0 : 1);
            UI.DrawTexture(sB, texture, drawPos, sourceRectangle: frame, effects: flip ? SpriteEffects.FlipVertically : SpriteEffects.None, scale: _scale[index], color: Color.White * alpha);
            if (!_tick[index]) {
                SoundEngine.PlaySound(SoundID.MenuTick);
                _tick[index] = true;
            };
            UI.DrawMouseText(sB, text, active ? Color.White : Color.Gray);
            if (UI.LeftClick() && active) {
                action.Invoke();
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
        } else {
            _scale[index] = MathHelper.Clamp(_scale[index] - 0.01f, 1f, 1.13f);
            _tick[index] = false; 
        };
    }
    void Init(RaceInfo race, string info, string pozitiv, string negativ) {
        race ??= new Human();
        _clon = race;

        _visibilityText.Clear();
        _alltext.Clear();

        foreach (string line in info.Split('\n')) { _alltext.Add((line + "\n", Color.White)); };
        foreach (string line in pozitiv.Split('\n')) { _alltext.Add((line + "\n", Color.Green)); };
        foreach (string line in negativ.Split('\n')) { _alltext.Add((line + "\n", Color.Red)); };

        _visibilityText = [.. _alltext];
        _removeIndex = -1;
    }
};