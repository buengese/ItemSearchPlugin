using System;
using System.Linq;
using System.Numerics;
using Lumina.Excel.GeneratedSheets;
using ImGuiNET;

namespace ItemSearch2.Filters {
    class LevelItemSearchFilter : SearchFilter {
        private int MinLevel = 1;
        private int MaxLevel = 700;

        private int minLevel;
        private int maxLevel;

        private int lastMinLevel;
        private int lastMaxLevel;

        public LevelItemSearchFilter() {
            minLevel = lastMinLevel = MinLevel;
            maxLevel = lastMaxLevel = MaxLevel;
        }

        public override string Name => "Item Level";

        public override string NameLocalizationKey => "SearchFilterLevelItem";

        public override bool IsSet => minLevel != MinLevel || maxLevel != MaxLevel;

        public override bool HasChanged {
            get {
                if (Modified || minLevel != lastMinLevel || maxLevel != lastMaxLevel) {
                    lastMaxLevel = maxLevel;
                    lastMinLevel = minLevel;
                    Modified = false;
                    return true;
                }

                return false;
            }
        }

        public override bool CheckFilter(Item item) {
            return item.LevelItem.Row >= minLevel && item.LevelItem.Row <= maxLevel;
        }

        public override void DrawEditor() {
            ImGui.BeginChild($"{NameLocalizationKey}Child", new Vector2(-1, 23 * ImGui.GetIO().FontGlobalScale), false, ImGuiWindowFlags.None);

            ImGui.PushItemWidth(-1);
            var min =  minLevel;
            var max =  maxLevel;


            if (ImGui.DragIntRange2("##LevelItemSearchFilterRange", ref min, ref max, 1f, MinLevel, MaxLevel)) {
                minLevel = min;
                maxLevel = max;
                // Force ImGui to behave
                // https://cdn.discordapp.com/attachments/653504487352303619/713825323967447120/ehS7GdAHKG.gif
                if (minLevel > maxLevel && minLevel != lastMinLevel) minLevel = maxLevel;
                if (maxLevel < minLevel && maxLevel != lastMaxLevel) maxLevel = minLevel;
                if (minLevel < MinLevel) minLevel = MinLevel;
                if (maxLevel > MaxLevel) maxLevel = MaxLevel;
            }

            ImGui.PopItemWidth();
            ImGui.EndChild();

        }
        
        public override string ToString() {
            return $"{minLevel} - { maxLevel}";
        }
    }
}
