using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Data;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Data;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class SoldByNPCSearchFilter : SearchFilter {
        private readonly HashSet<uint> soldForAnything = new HashSet<uint>();
        private readonly Dictionary<uint, HashSet<uint>> soldForCurrency = new Dictionary<uint, HashSet<uint>>();
        
        private enum SpecialCurrency : uint {
            [Description("Grand Company Seal")]
            GrandCompany = uint.MaxValue - 10,
            [Description("Beast Tribe Currency")]
            BeastTribe,

        }


        private class CurrencyOption {
            public bool Invert;
            public HashSet<uint> ItemHashSet = new HashSet<uint>();
            public string Name = string.Empty;
            public List<CurrencyOption> SubOptions = new List<CurrencyOption>();
            public bool HideIfEmpty = true;
        }

        private uint[] beastTribeCurrencies;

        private bool ready;
        private bool error;


        public SoldByNPCSearchFilter() {
            var notSoldByNpcOption = new CurrencyOption {Invert = true, Name = "Not sold by NPC", ItemHashSet = soldForAnything, HideIfEmpty = false};
            var soldByAnyNpcOption = new CurrencyOption {Name = "Any Currency", ItemHashSet = soldForAnything, HideIfEmpty = false};


            availableOptions.Add(null); // Not Selected Option
            availableOptions.Add(notSoldByNpcOption);
            availableOptions.Add(soldByAnyNpcOption);
            
            availableOptions.Add(GetCurrencyOption(1, "gil"));
            availableOptions.Add(GetCurrencyOption(29, "mgp"));

            soldForCurrency.Add((uint)SpecialCurrency.GrandCompany, new HashSet<uint>()); // Grand Company Seal
            availableOptions.Add(new CurrencyOption {
                Name = "Grand Company Seals",
                ItemHashSet = soldForCurrency[(uint)SpecialCurrency.GrandCompany],
                SubOptions = GetGrandCompanyCurrencies()
            });

            soldForCurrency.Add((uint)SpecialCurrency.BeastTribe, new HashSet<uint>()); // Beast Tribe
            availableOptions.Add(new CurrencyOption {
                Name = "Beast Tribe Currencies",
                ItemHashSet = soldForCurrency[(uint)SpecialCurrency.BeastTribe],
                SubOptions = GetBeastTribeCurrencies()
            });

            availableOptions.Add(GetCurrencyOption(28, "poetics"));
            availableOptions.Add(GetCurrencyOption(40, "allegory"));
            availableOptions.Add(GetCurrencyOption(41, "revelation"));

            Task.Run(() => {
                try {
                    foreach (var gilShopItem in Service.Data.Excel.GetSheet<GilShopItem>()) {
                        if (!soldForAnything.Contains(gilShopItem.Item.Row)) soldForAnything.Add(gilShopItem.Item.Row);
                        if (!soldForCurrency[1].Contains(gilShopItem.Item.Row)) soldForCurrency[1].Add(gilShopItem.Item.Row);
                    }

                    foreach (var gcScripShopItem in Service.Data.Excel.GetSheet<GCScripShopItem>()) {
                        if (!soldForAnything.Contains(gcScripShopItem.Item.Row)) soldForAnything.Add(gcScripShopItem.Item.Row);
                        if (!soldForCurrency[(uint)SpecialCurrency.GrandCompany].Contains(gcScripShopItem.Item.Row)) soldForCurrency[(uint)SpecialCurrency.GrandCompany].Add(gcScripShopItem.Item.Row);

                        var gcScripShopCategory = Service.Data.Excel.GetSheet<GCScripShopCategory>().GetRow(gcScripShopItem.RowId);
                        if (gcScripShopCategory == null) continue;
                        var grandCompanyID = gcScripShopCategory.GrandCompany.Row;
                        if (grandCompanyID < 1 || grandCompanyID > 3) continue;
                        if (!soldForCurrency[19 + grandCompanyID].Contains(gcScripShopItem.Item.Row)) soldForCurrency[19 + grandCompanyID].Add(gcScripShopItem.Item.Row);
                    }

                    foreach (var specialShop in Service.Data.Excel.GetSheet<SpecialShopCustom>()) {
                        foreach (var entry in specialShop.Entries) {
                            foreach (var c in entry.Cost) {
                                if (!soldForCurrency.ContainsKey(c.Item.Row)) continue;
                                foreach (var r in entry.Result) {
                                    if (!soldForAnything.Contains(r.Item.Row)) soldForAnything.Add(r.Item.Row);
                                    if (beastTribeCurrencies.Contains(c.Item.Row)) {
                                        if (!soldForCurrency[(uint)SpecialCurrency.BeastTribe].Contains(r.Item.Row)) soldForCurrency[(uint)SpecialCurrency.BeastTribe].Add(r.Item.Row);
                                    }
                                    if (!soldForCurrency[c.Item.Row].Contains(r.Item.Row)) soldForCurrency[c.Item.Row].Add(r.Item.Row);
                                }
                            }
                        }
                    }

                    availableOptions.RemoveAll(o => {
                        if (o == null) return false;
                        if (!o.HideIfEmpty) return false;
                        return o.ItemHashSet.Count == 0;
                    });

                    availableOptions.ForEach(o => {
                        if (o == null) return;
                        o.SubOptions.RemoveAll(so => {
                            if (so == null) return false;
                            if (!so.HideIfEmpty) return false;
                            return so.ItemHashSet.Count == 0;
                        });
                    });

                    ready = true;

                } catch (Exception ex) {
                    error = true;
                    PluginLog.LogError($"{ex}");
                }

                
            });
            
        }

        private List<CurrencyOption> GetBeastTribeCurrencies() {
            var l = new List<CurrencyOption>() {null};
            var a = new List<uint>();
            var btSheet = Service.Data.Excel.GetSheet<BeastTribe>();
            foreach (var bt in Service.Data.Excel.GetSheet<BeastTribe>()) {
                if (bt.CurrencyItem.Row == 0) continue;
                var co = GetCurrencyOption(bt.CurrencyItem.Row, bt.Name);
                if (co == null) continue;
                string name = bt.Name;
                if (btSheet.RequestedLanguage == Language.English) {
                    name = $"{name.Substring(0, 1).ToUpper()}{name.Substring(1)}";
                }

                co.Name = $"{name} / {co.Name}";

                a.Add(bt.CurrencyItem.Row);
                l.Add(co);
            }

            beastTribeCurrencies = a.ToArray();
            return l;
        }

        private List<CurrencyOption> GetGrandCompanyCurrencies() {
            var l = new List<CurrencyOption>() {null};
            foreach (var gc in Service.Data.Excel.GetSheet<GrandCompany>()) {
                if (gc.RowId == 0) continue;
                var co = GetCurrencyOption(19 + gc.RowId, gc.Name, gc.Name);
                if (co == null) continue;
                l.Add(co);
            }
            return l;
        }

        private CurrencyOption GetCurrencyOption(uint itemId, string tag = null, string forceName = null) {
            try {
                if (!soldForCurrency.ContainsKey(itemId)) {
                    soldForCurrency.Add(itemId, new HashSet<uint>());
                }
                var sheet = Service.Data.Excel.GetSheet<Item>();
                var item = sheet.GetRow(itemId);
                if (item == null) {
                    return new CurrencyOption()
                    {
                        Name = forceName ?? itemId.ToString(), 
                        ItemHashSet = soldForCurrency[itemId]
                    };
                }
                return new CurrencyOption()
                {
                    Name = forceName ?? item.Name,
                    ItemHashSet = soldForCurrency[itemId]
                };
            } catch (Exception ex) {
                PluginLog.Log($"Failed to get Currency Option for {itemId}");
                PluginLog.LogError($"{ex}");
                return null;
            }
            
        }
        
        private readonly List<CurrencyOption> availableOptions = new List<CurrencyOption>();
        private CurrencyOption selectedCurrencyOption;
        private CurrencyOption selectedSubOption;


        public override string Name => "Sold by NPC";
        public override string NameLocalizationKey => "SoldByNPCSearchFilter";

        public override bool IsSet => selectedCurrencyOption != null;


        public override bool CheckFilter(Item item) {
            while (!ready && !error) Thread.Sleep(1);
            if (error) return true;
            var option = selectedCurrencyOption;

            if (selectedSubOption != null) {
                option = selectedSubOption;
            }

            if (option == null) return true;

            if (option.Invert) {
                return !option.ItemHashSet.Contains(item.RowId);
            } else {
                return option.ItemHashSet.Contains(item.RowId);
            }
        }

        public override void DrawEditor() {
            if (error) {
                ImGui.Text("Error");
                return;
            }
            if (!ready) {
                ImGui.Text("Loading...");
                return;
            }
            ImGui.BeginChild($"###{NameLocalizationKey}Child", new Vector2(-1, 23 * ImGui.GetIO().FontGlobalScale), false, ImGuiWindowFlags.None);

            if (selectedCurrencyOption != null && selectedCurrencyOption.SubOptions.Count > 0) {
                ImGui.SetNextItemWidth(ImGui.GetWindowContentRegionWidth() / 2);
            } else {
                ImGui.SetNextItemWidth(-1);
            }
            
            if (ImGui.BeginCombo("###SoldbyNPCSearchFilter_selection", selectedCurrencyOption?.Name ?? "Not Selected")) {

                foreach (var option in availableOptions) {

                    if (ImGui.Selectable(option?.Name ?? "Not Selected", selectedCurrencyOption == option)) {
                        selectedCurrencyOption = option;
                        selectedSubOption = null;
                        Modified = true;
                    }

                }

                ImGui.EndCombo();
            }

            if (selectedCurrencyOption != null && selectedCurrencyOption.SubOptions.Count > 0) {
                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);
                if (ImGui.BeginCombo("###SoldbyNPCSearchFilter_subselection", selectedSubOption?.Name ?? "Any")) {

                    foreach (var option in selectedCurrencyOption.SubOptions) {
                        if (ImGui.Selectable(option?.Name ?? "Any", selectedCurrencyOption == option)) {
                            selectedSubOption = option;
                            Modified = true;
                        }
                    }
                    ImGui.EndCombo();
                }
            }


            ImGui.EndChild();
        }
        
    }
}
