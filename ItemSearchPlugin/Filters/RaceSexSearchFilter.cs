using Dalamud.Data;
using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static ItemSearchPlugin.ClassExtensions;

namespace ItemSearchPlugin.Filters {
    internal class RaceSexSearchFilter : SearchFilter {
        private int selectedOption;
        private int lastIndex;
        private readonly List<(string text, uint raceId, CharacterSex sex)> options;
        private readonly List<EquipRaceCategory> equipRaceCategories;
        
        public RaceSexSearchFilter() {
            equipRaceCategories = Service.Data.GetExcelSheet<EquipRaceCategory>().ToList();

            options = new List<(string text, uint raceId, CharacterSex sex)> {
                (Loc.Localize("NotSelected", "Not Selected"), 0, CharacterSex.Female)
            };

            foreach (var race in Service.Data.GetExcelSheet<Race>().ToList()) {
                if (race.RSEMBody.Row > 0 && race.RSEFBody.Row > 0) {
                    string male = string.Format(Loc.Localize("RaceSexMale", "Male {0}"), race.Masculine);
                    string female = string.Format(Loc.Localize("RaceSexFemale", "Female {0}"), race.Feminine);
                    options.Add((male, race.RowId, CharacterSex.Male));
                    options.Add((female, race.RowId, CharacterSex.Female));
                } else if (race.RSEMBody.Row > 0) {
                    options.Add((race.Masculine, race.RowId, CharacterSex.Male));
                } else if (race.RSEFBody.Row > 0) {
                    options.Add((race.Feminine, race.RowId, CharacterSex.Female));
                }
            }
        }

        public override string Name => "Sex / Race";

        public override string NameLocalizationKey => "RaceSexSearchFilter";

        public override bool IsSet => selectedOption > 0;

        public override bool HasChanged {
            get {
                if (lastIndex == selectedOption) return false;
                lastIndex = selectedOption;
                return true;
            }
        }

        public override bool CheckFilter(Item item) {
            try {
                var (_, raceId, sex) = options[selectedOption];
                var erc = equipRaceCategories[item.EquipRestriction];
                return erc.AllowsRaceSex(raceId, sex);
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
                return true;
            }
        }

        public override void DrawEditor() {
            ImGui.BeginChild($"{this.NameLocalizationKey}Child", new Vector2(-1, 23 * ImGui.GetIO().FontGlobalScale), false, ImGuiWindowFlags.None);
            if (Service.ClientState?.LocalContentId != 0) {
                ImGui.SetNextItemWidth(-80 * ImGui.GetIO().FontGlobalScale);
            } else {
                ImGui.SetNextItemWidth(-1);
            }
            
            ImGui.Combo("##RaceSexSearchFilter", ref this.selectedOption, options.Select(a => a.text).ToArray(), options.Count);

            if (Service.ClientState?.LocalContentId != 0) {
                ImGui.SameLine();
                
                if (ImGui.SmallButton($"Current")) {
                    if (Service.ClientState?.LocalPlayer != null) {
                        var race = Service.ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Race];
                        var sex = Service.ClientState.LocalPlayer.Customize[(int)CustomizeIndex.Gender] == 0 ? CharacterSex.Male : CharacterSex.Female;

                        for (var i = 0; i < options.Count; i++) {
                            if (options[i].sex == sex && options[i].raceId == race) {
                                selectedOption = i;
                                break;
                            }
                        }
                    }
                }
            }
            ImGui.EndChild();
            
        }

        public override string ToString() {
            return options[selectedOption].text;
        }
    }
}
