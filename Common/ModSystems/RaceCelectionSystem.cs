using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Waybound.Common.ModSystems
{
    public class RaceSelectionSystem : ModSystem
    {
        public static string SelectedRace = "Human";
        public static readonly string[] Races = { "Dwarf", "Viking", "Lihzard", "Desfo", "Human" };

        public static RaceSelectionUI RaceUI;
        private static bool _isRaceSelectionActive = false;
        private static bool _hasIntercepted = false;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                RaceUI = new RaceSelectionUI();

                Main.OnTickForThirdPartySoftwareOnly += OnMenuTick;
            }
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                Main.OnTickForThirdPartySoftwareOnly -= OnMenuTick;
                RaceUI = null;
            }
        }

        private void OnMenuTick()
        {
            if (Main.menuMode == 2 && !_isRaceSelectionActive && !_hasIntercepted)
            {
                if (Main.MenuUI.CurrentState is UICharacterCreation)
                {
                    _isRaceSelectionActive = true;
                    _hasIntercepted = true;

                    Main.menuMode = 888;
                    Main.MenuUI.SetState(RaceUI);
                }
            }

            if (Main.menuMode == 1 || Main.menuMode == 0)
            {
                _isRaceSelectionActive = false;
                _hasIntercepted = false;
            }
        }

        public static void GoToVanillaCharacterCreation()
        {
            if (Main.PendingPlayer == null)
            {
                Main.PendingPlayer = new Player();
            }

            var creationUI = new UICharacterCreation(Main.PendingPlayer);
            Main.menuMode = 2;
            Main.MenuUI.SetState(creationUI);

            _isRaceSelectionActive = false;
            _hasIntercepted = true; 
        }

        public static void GoBackToCharacterSelect()
        {
            Main.menuMode = 1;
            Main.MenuUI.SetState(new UICharacterSelect());
            _isRaceSelectionActive = false;
            _hasIntercepted = false;
        }
    }

    public class RaceSelectionUI : UIState
    {
        private UIPanel[] _racePanels;
        private UITextPanel<string> _nextButton;
        private UITextPanel<string> _backButton;
        private UIPanel _mainPanel;

        public override void OnInitialize()
        {
            _mainPanel = new UIPanel();
            _mainPanel.Width.Set(600f, 0f);
            _mainPanel.Height.Set(550f, 0f);
            _mainPanel.HAlign = 0.5f;
            _mainPanel.VAlign = 0.5f;
            _mainPanel.BackgroundColor = new Color(30, 30, 40);
            _mainPanel.BorderColor = new Color(80, 80, 100);
            Append(_mainPanel);

            UIText title = new UIText("Выберите расу", 1.5f, true);
            title.HAlign = 0.5f;
            title.Top.Set(20f, 0f);
            _mainPanel.Append(title);

            UIElement listContainer = new UIElement();
            listContainer.Width.Set(400f, 0f);
            listContainer.Height.Set(350f, 0f);
            listContainer.HAlign = 0.5f;
            listContainer.Top.Set(70f, 0f);
            _mainPanel.Append(listContainer);

            _racePanels = new UIPanel[RaceSelectionSystem.Races.Length];
            float panelH = 65f;
            float spacing = 8f;

            for (int i = 0; i < RaceSelectionSystem.Races.Length; i++)
            {
                string race = RaceSelectionSystem.Races[i];

                UIPanel panel = new UIPanel();
                panel.Width.Set(380f, 0f);
                panel.Height.Set(panelH, 0f);
                panel.Left.Set(10f, 0f);
                panel.Top.Set(i * (panelH + spacing), 0f);
                panel.BackgroundColor = new Color(40, 40, 55);
                panel.BorderColor = Color.Gray;

                panel.OnMouseOver += (_, __) =>
                {
                    if (RaceSelectionSystem.SelectedRace != race)
                    {
                        panel.BackgroundColor = new Color(70, 70, 100);
                        panel.BorderColor = Color.White;
                    }
                    SoundEngine.PlaySound(SoundID.MenuTick);
                };

                panel.OnMouseOut += (_, __) => UpdatePanelColors();

                panel.OnLeftClick += (_, __) =>
                {
                    RaceSelectionSystem.SelectedRace = race;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    UpdatePanelColors();
                };

                UIText text = new UIText(race, 1.2f);
                text.HAlign = 0.5f;
                text.VAlign = 0.5f;
                panel.Append(text);

                _racePanels[i] = panel;
                listContainer.Append(panel);
            }
            
            _nextButton = new UITextPanel<string>("Далее →", 1.15f, true);
            _nextButton.Width.Set(180f, 0f);
            _nextButton.Height.Set(45f, 0f);
            _nextButton.HAlign = 0.5f;
            _nextButton.Top.Set(440f, 0f);
            _nextButton.BackgroundColor = new Color(40, 110, 40);
            _nextButton.BorderColor = new Color(60, 160, 60);

            _nextButton.OnMouseOver += (_, __) =>
            {
                _nextButton.BackgroundColor = new Color(60, 160, 60);
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            _nextButton.OnMouseOut += (_, __) => _nextButton.BackgroundColor = new Color(40, 110, 40);
            _nextButton.OnLeftClick += (_, __) =>
            {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                RaceSelectionSystem.GoToVanillaCharacterCreation();
            };
            _mainPanel.Append(_nextButton);

            _backButton = new UITextPanel<string>("← Назад", 1f, true);
            _backButton.Width.Set(140f, 0f);
            _backButton.Height.Set(40f, 0f);
            _backButton.HAlign = 0.5f;
            _backButton.Top.Set(495f, 0f);
            _backButton.BackgroundColor = new Color(90, 40, 40);
            _backButton.BorderColor = new Color(130, 50, 50);

            _backButton.OnMouseOver += (_, __) =>
            {
                _backButton.BackgroundColor = new Color(130, 50, 50);
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            _backButton.OnMouseOut += (_, __) => _backButton.BackgroundColor = new Color(90, 40, 40);
            _backButton.OnLeftClick += (_, __) =>
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                RaceSelectionSystem.GoBackToCharacterSelect();
            };
            _mainPanel.Append(_backButton);

            UpdatePanelColors();
        }

        public override void OnActivate()
        {
            UpdatePanelColors();
        }

        private void UpdatePanelColors()
        {
            if (_racePanels == null) return;

            for (int i = 0; i < _racePanels.Length; i++)
            {
                if (_racePanels[i] == null) continue;

                bool selected = RaceSelectionSystem.SelectedRace == RaceSelectionSystem.Races[i];
                _racePanels[i].BackgroundColor = selected
                    ? new Color(100, 65, 25)
                    : new Color(40, 40, 55);
                _racePanels[i].BorderColor = selected ? Color.Gold : Color.Gray;
            }
        }
    }
}