﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Data;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearch2.Filters {
    internal class StatSearchFilter : SearchFilter {
        public class Stat {
            public BaseParam BaseParam;
            public int BaseParamIndex;
        }

        private BaseParam[] baseParams;

        private bool modeAny;


        public readonly List<Stat> Stats = new();

        public StatSearchFilter() {
            Task.Run(() => {
                var baseParamCounts = new Dictionary<byte, int>();

                foreach (var p in Service.Data.GetExcelSheet<Item>().ToList().SelectMany(i => i.UnkData59)) {
                    if (!baseParamCounts.ContainsKey(p.BaseParam)) {
                        baseParamCounts.Add(p.BaseParam, 0);
                    }

                    baseParamCounts[p.BaseParam] += 1;
                }

                var sheet = Service.Data.GetExcelSheet<BaseParam>();
                baseParams = baseParamCounts.OrderBy(p => p.Value).Reverse().Select(pair => sheet.GetRow(pair.Key)).ToArray();
            });
        }

        public override string Name => "Has Stats";
        public override string NameLocalizationKey => "StatSearchFilter";
        public override bool IsSet => Stats.Count > 0 && Stats.Any(s => s.BaseParam != null && s.BaseParam.RowId != 0);

        public override bool CheckFilter(Item item) {
            if (baseParams == null) return true;
            if (modeAny) {
                // Match Any
                foreach (var s in Stats.Where(s => s.BaseParam != null && s.BaseParam.RowId != 0)) {
                    foreach (var p in item.UnkData59) {
                        if (p.BaseParam == s.BaseParam.RowId) {
                            return true;
                        }
                    }
                }

                return false;
            } else {
                // Match All

                foreach (var s in Stats.Where(s => s.BaseParam != null && s.BaseParam.RowId != 0)) {
                    bool foundMatch = false;
                    foreach (var p in item.UnkData59) {
                        if (p.BaseParam == s.BaseParam.RowId) {
                            foundMatch = true;
                        }
                    }

                    if (!foundMatch) {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void DrawEditor() {
            var btnSize = new Vector2(24 * ImGui.GetIO().FontGlobalScale);

            if (baseParams == null) {
                // Still loading
                ImGui.Text("");
                return;
            }


            Stat doRemove = null;
            var i = 0;
            foreach (var stat in Stats) {
                if (ImGui.Button($"-###statSearchFilterRemove{i++}", btnSize)) doRemove = stat;
                
                var selectedParam = stat.BaseParamIndex;
                ImGui.SetNextItemWidth(200);


                ImGui.SameLine();
                if (ImGui.Combo($"###statSearchFilterSelectStat{i++}", ref selectedParam, baseParams.Select(bp => bp.RowId == 0 ? Loc.Localize("StatSearchFilterSelectStat", "Select a stat...") : bp.Name).ToArray(), baseParams.Length, 20)) {
                    stat.BaseParamIndex = selectedParam;
                    stat.BaseParam = baseParams[selectedParam];
                    Modified = true;
                }
            }

            if (doRemove != null) {
                Stats.Remove(doRemove);
                Modified = true;
            }

            if (ImGui.Button("+", btnSize)) {
                var stat = new Stat();
                Stats.Add(stat);
                Modified = true;
            }

            if (Stats.Count > 1) {
                ImGui.SameLine();
                if (ImGui.Checkbox($"{Loc.Localize("StatSearchFilterMatchAny", "Match Any")}###StatSearchFilterShowAny", ref modeAny)) {
                    Modified = true;
                }
            }
        }
        
        public override string ToString() {
            return string.Join(", ", Stats.Select(s => s.BaseParam.Name));
        }
    }
}
