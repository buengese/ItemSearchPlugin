﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Data;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearch2.Filters {
    internal class CraftableSearchFilter : SearchFilter {
        private int selectedOption = 0;

        private readonly string[] options;

        private readonly Dictionary<uint, RecipeLookup> craftableItems;

        private bool finishedLoading = false;
        
        public CraftableSearchFilter() {
            this.craftableItems = new Dictionary<uint, RecipeLookup>();
            var craftableJobFormat = Loc.Localize("CraftableJobFormat", "Craftable: {0}");

            options = new string[11];

            options[0] = "";
            options[1] = Loc.Localize("NotCraftable", "Not Craftable");
            options[2] = string.Format(craftableJobFormat, Loc.Localize("SearchFilterAny", "Any"));
            
            Task.Run(() => {
                var cj = Service.Data.GetExcelSheet<ClassJob>();

                try
                {
                    for (uint i = 0; i < 8; i++)
                    {
                        var job = cj!.GetRow(i + 8);
                        options[3 + i] = string.Format(craftableJobFormat, job!.Abbreviation);
                    }

                    foreach (var recipeLookup in Service.Data.GetExcelSheet<RecipeLookup>()!)
                    {
                        craftableItems.Add(recipeLookup.RowId, recipeLookup);
                    }
                }
                catch (NullReferenceException ex)
                {
                    PluginLog.Error(ex, "CraftableSearchFilter load failed");
                }

                finishedLoading = true;
                Modified = true;
            });
        }

        public override string Name { get; } = "Craftable";
        public override string NameLocalizationKey { get; } = "CraftableSearchFilter";
        public override bool IsSet => selectedOption > 0;

        public override bool ShowFilter => base.ShowFilter && finishedLoading;

        public override bool CheckFilter(Item item) {
            if (item == null) return false;
            if (!finishedLoading) return true;

            var isCraftable = craftableItems.ContainsKey(item.RowId);
            return selectedOption switch {
                1 => !isCraftable,
                2 => isCraftable,
                3 => isCraftable && craftableItems[item.RowId].CRP.Row > 0,
                4 => isCraftable && craftableItems[item.RowId].BSM.Row > 0,
                5 => isCraftable && craftableItems[item.RowId].ARM.Row > 0,
                6 => isCraftable && craftableItems[item.RowId].GSM.Row > 0,
                7 => isCraftable && craftableItems[item.RowId].LTW.Row > 0,
                8 => isCraftable && craftableItems[item.RowId].WVR.Row > 0,
                9 => isCraftable && craftableItems[item.RowId].ALC.Row > 0,
                10 => isCraftable && craftableItems[item.RowId].CUL.Row > 0,
                _ => true
            };
        }

        public override void DrawEditor() {
            ImGui.BeginChild($"###{NameLocalizationKey}Child", new Vector2(-1, 23 * ImGui.GetIO().FontGlobalScale), false, ImGuiWindowFlags.None);
            ImGui.SetNextItemWidth(-1);
            if (ImGui.Combo("###craftableSearchFilter_selection", ref selectedOption, options, options.Length, 14)) {
                Modified = true;
            }
            ImGui.EndChild();
        }


        
        public override string ToString() {
            return options[selectedOption].Replace("Craftable: ", "");
        }
    }
}
