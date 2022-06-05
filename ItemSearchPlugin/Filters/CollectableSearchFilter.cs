using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.Filters {
    class CollectableSearchFilter : SearchFilter {

        public enum Mode {
            [Description("Not Selected")]
            NotSelected,
            [Description("Any Collectable")]
            AnyCollectable,
            [Description("Not Collectable")]
            NotCollectable,
            [Description("Owned Collectable")]
            OwnedCollectable,
            [Description("Unowned Collectable")]
            UnownedCollectable
        }

        private Mode selectedMode = Mode.NotSelected;
        
        private GameFunctions GameFunctions { get; }
        
        public override string Name => "Collectable";
        public override string NameLocalizationKey => "CollectableSearchFilter";
        public override bool IsSet => Service.ClientState.LocalContentId != 0 && selectedMode != Mode.NotSelected;
        public override bool ShowFilter => Service.ClientState.LocalContentId != 0 && base.ShowFilter;

        private ushort[] collectableActionType = { 853, 1013, 1322, 2136, 2633, 3357, 4107, 25183, 20086 };

        private bool faultState = false;

        public CollectableSearchFilter(GameFunctions gameFunctions)
        {
            this.GameFunctions = gameFunctions;
        }

        public override bool CheckFilter(Item item) {
            if (faultState) return true;
            if (selectedMode == Mode.NotSelected) return true;
            var (isCollectable, isOwned) = GetCollectable(item);

            return selectedMode switch {
                Mode.NotCollectable => !isCollectable,
                Mode.AnyCollectable => isCollectable,
                Mode.OwnedCollectable => isCollectable && isOwned,
                Mode.UnownedCollectable => isCollectable && !isOwned,
                _ => true
            };
        }

        private (bool isCollectable, bool isOwned) GetCollectable(Item item) {
            
            var isCollectable = false;
            var isOwned = false;

            if (item == null) return (false, false);
            if (item.ItemAction == null || item.ItemAction.Row == 0) return (false, false);

            var actionId = item.ItemAction.Row;
            var actionType = item.ItemAction.Value?.Type ?? ushort.MaxValue;
            
            if (collectableActionType.Contains(actionType)) {
                isCollectable = true;
                isOwned = actionType == 3357 && GameFunctions.IsCardOwned((ushort) item.AdditionalData);
            }

            return (isCollectable, isOwned);
        }


        public override void DrawEditor() {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("###CollectableSearchFilterCombo", selectedMode.DescriptionAttr())) {
                foreach (var v in Enum.GetValues(typeof(Mode))) {
                    if (ImGui.Selectable(v.DescriptionAttr(), selectedMode == (Mode) v)) {
                        selectedMode = (Mode) v;
                        Modified = true;
                    }
                }
                ImGui.EndCombo();
            }
        }
    }
}
