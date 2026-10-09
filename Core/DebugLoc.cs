using Hjson;
using MonoMod.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Localization;

namespace Waybound.Core;

public class DebugLoc {
    readonly static Dictionary<(string, int), string> _debugLoc = [];

    public static void Load() {
        Stream stream;
        for (int i = 0; i < 9; i++) {
            try { stream = GetInstance<Waybound>().GetFileStream($"Localization/{GetLocName(i)}_Mods.Waybound.DebugInfo.hjson"); }
            catch (Exception) { stream = GetInstance<Waybound>().GetFileStream("Localization/en-US_Mods.Waybound.DebugInfo.hjson"); };
            using (stream) {
                using StreamReader reader = new(stream);
                Dictionary<string, Dictionary<string, string>> data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(HjsonValue.Parse(reader.ReadToEnd()).ToString());
                Dictionary<string, string> keyValue = [];
                keyValue.AddRange(data["Load"]);
                keyValue.AddRange(data["Error"]);
                foreach (KeyValuePair<string, string> value in keyValue) { _debugLoc.Add((value.Key, i), value.Value); };
            };
        };
    }
    static string GetLocName(int iD) {
        return iD switch {
            0 => "en-US",
            1 => "de-DE",
            2 => "it-IT",
            3 => "fr-FR",
            4 => "es-ES",
            5 => "ru-RU",
            6 => "zh-Hans",
            7 => "pt-BR",
            8 => "pl-PL",
            _ => "en-US",
        };
    }
    public static string GetLoc(string key) {
        if (_debugLoc.TryGetValue((key, Language.ActiveCulture.LegacyId - 1), out string value)) { return value; }
        else { throw new Exception(string.Format(_debugLoc[("InLock", Language.ActiveCulture.LegacyId - 1)], key)); };
    }
}
