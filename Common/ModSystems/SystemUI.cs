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
        public SystemUI()
        {
            Instance = this;
        }


        internal static UserInterface dialogueInterface;
        internal DialogueUI dialogueUI;

        internal static UserInterface titleInterface;
        internal TitleUI titleUI;

        public override void Load()
        {
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
            if (!Main.dedServ)
            {
                dialogueUI = null;
                titleUI = null;
            }
        }

        private GameTime _lastUpdateUiGameTime;

        public override void UpdateUI(GameTime gameTime)
        {

            if (dialogueInterface?.CurrentState != null)
            {
                dialogueInterface.Update(gameTime);
            }
            if (titleInterface?.CurrentState != null)
            {
                titleInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Insert(layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")), new LegacyGameInterfaceLayer("GUI Menus",
                delegate
                {
                    return true;
                }, InterfaceScaleType.UI));

            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {

                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VictimaMod2: dialogueInterface",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && dialogueInterface?.CurrentState != null)
                        {
                            dialogueInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }
                        return true;
                    },
                   InterfaceScaleType.UI));

                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VictimaMod2: titleInterface",
                    delegate
                    {
                        if (_lastUpdateUiGameTime != null && titleInterface?.CurrentState != null)
                        {
                            titleInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                        }
                        return true;
                    },
                   InterfaceScaleType.UI));
            }
        }
    }
}