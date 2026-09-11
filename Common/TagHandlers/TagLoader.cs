using Terraria.UI.Chat;

namespace Waybound.Common.TagHandlers;

internal class TagLoader {
    internal static void Load() {
        ChatManager.Register<Bar>("TBPreview"); // Bloody Necklace Bar

    }
};
