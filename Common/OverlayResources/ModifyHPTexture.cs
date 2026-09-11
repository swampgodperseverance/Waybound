using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;

namespace Waybound.Common.OverlayResources;

public class ModifyHPTexture : ModResourceOverlay {
    readonly Dictionary<string, Asset<Texture2D>> _vanillaAssetCache = [];

    ResourceOverlayDrawContext? _drawContext = null;

    string _currentName = "";

    bool _cursedText = false;

    
    public override bool DisplayHoverText(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife) {
        if (_cursedText) {
            Main.instance.MouseTextHackZoom(Loc.GetUI("ModifyHPTexture.Donated"));
            Main.mouseText = true;
            return false;
        } else { return true; };
    }
    public override void PostDrawResource(ResourceOverlayDrawContext context) {
        Player player = Main.LocalPlayer;
        if (player.GetModPlayer<BloodyNecklacePlayer>().CursedHP == -1) { return; };

        Asset<Texture2D> asset = context.texture;

        string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
        string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";
 
        if (asset == TextureAssets.Heart || asset == TextureAssets.Heart2) {
            if (context.resourceNumber == 0 && _currentName != context.DisplaySet.ConfigKey) {
                _drawContext = context;
                _currentName = context.DisplaySet.ConfigKey;
            }; return;
        } else if (CompareAssets(asset, fancyFolder + "Heart_Fill") || CompareAssets(asset, fancyFolder + "Heart_Fill_B")) {
            if (context.resourceNumber == 0 && _currentName != context.DisplaySet.ConfigKey) {
                _drawContext = context;
                _currentName = context.DisplaySet.ConfigKey;
            }; return;
        } else if (CompareAssets(asset, barsFolder + "HP_Fill") || CompareAssets(asset, barsFolder + "HP_Fill_Honey")) {
            if (context.resourceNumber == 0 && _currentName != context.DisplaySet.ConfigKey) {
                _drawContext = context;
                _currentName = context.DisplaySet.ConfigKey;
            }; return;
        };
    }
    bool CompareAssets(Asset<Texture2D> currentAsset, string compareAssetPath) {
        if (!_vanillaAssetCache.TryGetValue(compareAssetPath, out var asset)) asset = _vanillaAssetCache[compareAssetPath] = Main.Assets.Request<Texture2D>(compareAssetPath);
        return currentAsset == asset;
    }
};