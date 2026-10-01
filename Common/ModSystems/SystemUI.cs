using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Waybound.UIs;

namespace Waybound.Common.ModSystems
{
    public class SystemUI : ModSystem
    {
        public static SystemUI Instance { get; private set; }

        internal static UserInterface dialogueInterface;
        public static DialogueUI dialogueUI;

        internal static UserInterface titleInterface;
        public static TitleUI titleUI;

        private GameTime _lastUpdateUiGameTime;

        public override void Load()
        {
            Instance = this;

            if (!Main.dedServ)
            {
                dialogueInterface = new UserInterface();
                dialogueUI = new DialogueUI();
                dialogueInterface.SetState(dialogueUI);

                titleInterface = new UserInterface();
                titleUI = new TitleUI();
                titleInterface.SetState(titleUI);
            }
        }

        public override void Unload()
        {
            Instance = null;
            dialogueUI = null;
            titleUI = null;
            dialogueInterface = null;
            titleInterface = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _lastUpdateUiGameTime = gameTime;

            if (dialogueInterface?.CurrentState != null)
                dialogueInterface.Update(gameTime);

            if (titleInterface?.CurrentState != null)
                titleInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex == -1)
                return;

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Waybound: dialogueInterface",
                delegate
                {
                    if (_lastUpdateUiGameTime != null && dialogueInterface?.CurrentState != null)
                        dialogueInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);

                    return true;
                },
                InterfaceScaleType.UI));

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Waybound: titleInterface",
                delegate
                {
                    if (_lastUpdateUiGameTime != null && titleInterface?.CurrentState != null)
                        titleInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);

                    return true;
                },
                InterfaceScaleType.UI));
        }
    }
}