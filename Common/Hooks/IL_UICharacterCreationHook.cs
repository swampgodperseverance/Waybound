using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.UI;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;
using Waybound.Core;
using Waybound.UIs;

namespace Waybound.Common.Hooks;

internal static class IL_UICharacterCreationHook {
    internal static PlayerCreationData saveData = null;

    internal static void Load() {
        IL_UICharacterCreation.BuildPage += Init; // Init PlayerCreationData
        IL_UICharacterCreation.MakeBackAndCreatebuttons += DrawButton; // Draw Race Button
        IL_UICharacterCreation.Draw += Draw; // Draw !
        IL_UICharacterCreation.FinishCreatingCharacter += SaveRezultat; // Set player race
    }
    static void Init(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(i => i.MatchCallvirt("Terraria.UI.UIElement", "SetPadding"));
        c.Emit(OpCodes.Ldarg, 0);
        c.Emit(OpCodes.Ldloc, 1);
        c.EmitDelegate((UICharacterCreation self, UIElement element) => {
            Player player = (Player)typeof(UICharacterCreation).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            saveData = new(player);
        });
    }
    static void DrawButton(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(i => i.MatchLdcR4(0.5f));
        c.Remove();
        c.Emit(OpCodes.Ldc_R4, 0.35f);
        c.GotoNext(i => i.MatchLdcR4(0.5f));
        c.Remove();
        c.Emit(OpCodes.Ldc_R4, 0.35f);
        c.Index += 45;
        c.Emit(OpCodes.Ldarg, 0);
        c.Emit(OpCodes.Ldarg, 1);
        c.EmitDelegate((UICharacterCreation self, UIElement outerContainer) => {
            UITextPanel<LocalizedText> raceButton = new(Language.GetText(Loc.GetUI("PlayerRaceMenu.Create")), 0.7f, large: true) {
                Width = StyleDimension.FromPixelsAndPercent(-10f, 0.35f),
                Height = StyleDimension.FromPixels(50f),
                VAlign = 1f,
                HAlign = 0.5f,
                Top = StyleDimension.FromPixels(-45f)
            };

            raceButton.OnMouseOver += (evt, listeningElement) => {
                SoundEngine.PlaySound(SoundID.MenuTick);
                ((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
                ((UIPanel)evt.Target).BorderColor = Colors.FancyUIFatButtonMouseOver;
            };
            raceButton.OnMouseOut += (evt, listeningElement) => {
                ((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.8f;
                ((UIPanel)evt.Target).BorderColor = Color.Black;
            };

            raceButton.OnLeftClick += (evt, listeningElement) => {
                saveData.openRaceUI = !saveData.openRaceUI;

                Type type = typeof(UICharacterCreation);
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

                UIElement middleContainer = (UIElement)type.GetField("_middleContainer", flags).GetValue(self);
                UIElement topContainer = (UIElement)type.GetField("_topContainer", flags).GetValue(self);
                UIColoredImageButton clothingStyles = (UIColoredImageButton)type.GetField("_clothingStylesCategoryButton", flags).GetValue(self);
                UIColoredImageButton hairStyles = (UIColoredImageButton)type.GetField("_hairStylesCategoryButton", flags).GetValue(self);
                UIColoredImageButton charInfo = (UIColoredImageButton)type.GetField("_charInfoCategoryButton", flags).GetValue(self);

                if (saveData.openRaceUI) {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    saveData.raceConfirmUI?.Remove();
                    saveData.raceConfirmUI = null;
                    saveData.element = new Race(saveData);
                    saveData.element.OnInitialize();
                    saveData.element.Append(new UIText(Language.GetText(Loc.GetUI("PlayerRaceMenu.MainPage")), 0.55f, true) {
                        Left = StyleDimension.FromPixels(152f),
                        Top = StyleDimension.FromPixels(62f)
                    });
                    outerContainer.Append(saveData.element);
                    if (saveData.middleContainer == null) {
                        saveData.middleContainer = middleContainer;
                        saveData.topContainer = topContainer;
                        saveData.parent = middleContainer.Parent;
                    };
                    clothingStyles.SetSelected(false);
                    hairStyles.SetSelected(false);
                    charInfo.SetSelected(false);
                    middleContainer.Remove();
                    topContainer.Remove();
                }
                else {
                    SoundEngine.PlaySound(SoundID.MenuClose);
                    saveData.element?.Remove();
                    saveData.element = null;
                    if (saveData.parent != null) {
                        saveData.parent.Append(saveData.topContainer);
                        saveData.parent.Append(saveData.middleContainer);
                    };
                };
            };

            raceButton.SetSnapPoint("Race", 0);
            outerContainer.Append(raceButton);
        });
    }
    static void Draw(ILContext il) {
        ILCursor c = new(il);
        c.Emit(OpCodes.Ldarg, 1);
        c.EmitDelegate((SpriteBatch sB) => {         
            if (saveData.openRaceUI) {
            }
        });
    }
    static void SaveRezultat(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(MoveType.Before, i => i.MatchCall(typeof(PlayerFileData), nameof(PlayerFileData.CreateAndSave)));
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((UICharacterCreation self) => {
            Player player = (Player)typeof(UICharacterCreation).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            player.GetModPlayer<RacePlayer>().race = saveData.race;
        });
    }
    internal static void Unload() {
        IL_UICharacterCreation.BuildPage -= Init;
        IL_UICharacterCreation.MakeBackAndCreatebuttons -= DrawButton;
        IL_UICharacterCreation.Draw -= Draw;
    }
}