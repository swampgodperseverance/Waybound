using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.WUtils;
using Waybound.Content.NPCs.TownNPCs;

namespace Waybound.Content.Items.Lore.Notes
{
    /// here we have an abstract class for every lore note that the crystal guardian accepts through UI

    public static class NoteGroups
    {
        public const string Desert = "Desert";
        public const string Jungle = "Jungle";
        public const string Snow = "Snow";
        public const string Crystal = "Crystal";
        public const string Ocean = "Ocean";
    }
    public abstract class LoreNote : ModItem
    {
        // Big title that shown before the lines
        public abstract string NoteTitle { get; }

        // keep each line under ~65 characters
        public abstract string[] Lines { get; }

        // name above line, null = nothing
        public virtual string Speaker => null;

        public virtual Color TextColor => new Color(255, 105, 150, 0);   
        public virtual Color ShadowColor => new Color(6, 34, 24);     

        public virtual void OnFirstRead(Player player) { }
        public virtual string Group => null;
        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 26;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 5, 0);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            bool read = Main.LocalPlayer.GetModPlayer<GuardianPlayer>().HasRead(this);

            tooltips.Add(new TooltipLine(Mod, "NoteStatus",
                read
                    ? Loc.GetTips("CrystalNote.Read")
                    : Loc.GetTips("CrystalNote.Unread"))
            {
                OverrideColor = read
                    ? new Color(150, 215, 255)
                    : new Color(255, 220, 140)
            });
        }

        public override void PostUpdate() => Lighting.AddLight(Item.Center, 0.15f, 0.3f, 0.5f);
    }
}