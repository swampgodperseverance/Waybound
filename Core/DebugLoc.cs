using MonoMod.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Localization;

namespace Waybound.Core;
public class DebugLoc {
    readonly static Dictionary<string, string> _debugLoc = [];

    public static void Load() {
        Stream stream;
        try { stream = GetInstance<Waybound>().GetFileStream($"Localization/{Language.ActiveCulture.Name}_Mods.Waybound.DebugInfo.json"); }
        catch (Exception) { stream = GetInstance<Waybound>().GetFileStream("Localization/en-US_Mods.Waybound.DebugInfo.json"); };
        using (stream) {
            using StreamReader reader = new(stream);
            Dictionary<string, Dictionary<string, string>> data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(reader.ReadToEnd());
            _debugLoc.AddRange(data["Load"]);
            _debugLoc.AddRange(data["Error"]);
        };
    }
    public static string GetLoc(string key) {
        if (_debugLoc.TryGetValue(key, out string value)) { return value; }
        else { throw new Exception(string.Format(_debugLoc["InLock"], key)); };
    }
}
