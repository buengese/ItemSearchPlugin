using Lumina.Excel.GeneratedSheets;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Plugin;

namespace ItemSearch2.Filters {
    class ItemNameSearchFilter : SearchFilter {
        private string searchText;
        private string lastSearchText;

        public ItemNameSearchFilter(ItemSearchWindow window, string startingValue = "") {
            searchText = startingValue;
            lastSearchText = string.Empty;
        }
        
        public override string Name => "Search";

        public override string NameLocalizationKey => "DalamudItemSearchVerb";

        public override bool IsSet => !string.IsNullOrEmpty(searchText);

        public override bool CanBeDisabled => false;
        

        public override bool HasChanged {
            get {
                if (searchText != lastSearchText) {
                    lastSearchText = searchText;
                    return true;
                }
                return false;
            }
        }

        /*public override bool CheckFilter(Item item) {
            if (searchRegex != null) {
                return searchRegex.IsMatch(item.Name);
            }

            return
                item.Name.ToString().ToLower().Contains(parsedSearchText.ToLower())
                || (searchTokens != null && searchTokens.Length > 0 && searchTokens.All(t => item.Name.ToString().ToLower().Contains(t)))
                || (int.TryParse(parsedSearchText, out var parsedId) && parsedId == item.RowId)
                || searchText.StartsWith("$") && item.Description.ToString().ToLower().Contains(parsedSearchText.Substring(1).ToLower());
        }*/

        public override bool CheckFilter(Item item)
        {
            return item.Name.ToString().ToUpperInvariant().Contains(searchText.ToUpperInvariant(), StringComparison.InvariantCulture);
        }

        /*public override bool CheckFilter(EventItem item) {
            if (searchRegex != null) {
                return searchRegex.IsMatch(item.Name);
            }

            return
                item.Name.ToString().ToLower().Contains(parsedSearchText.ToLower())
                || (searchTokens != null && searchTokens.Length > 0 && searchTokens.All(t => item.Name.ToString().ToLower().Contains(t)))
                || (int.TryParse(parsedSearchText, out var parsedId) && parsedId == item.RowId);
        }*/

        public override bool CheckFilter(EventItem item)
        {
            return item.Name.ToString().ToUpperInvariant().Contains(searchText.ToUpperInvariant(), StringComparison.InvariantCulture);
        }

        public override void DrawEditor() {
            ImGui.SetNextItemWidth(-20 * ImGui.GetIO().FontGlobalScale);
            if (Service.Configuration.AutoFocus && ImGui.IsWindowAppearing()) {
                ImGui.SetKeyboardFocusHere();
            }
            ImGui.InputText("##ItemNameSearchFilter", ref searchText, 256);
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered()) {
                ImGui.BeginTooltip();
                ImGui.Text("Type an item name to search for items by name.");
                ImGui.SameLine();
                ImGui.TextDisabled("\"OMG\"");
                ImGui.Text("Type an item id to search for item by its ID.");
                ImGui.SameLine();
                ImGui.TextDisabled("\"23991\"");
                ImGui.Text("Start input with '$' to search for an item by its description.");
                ImGui.SameLine();
                ImGui.TextDisabled("\"$Weird.\"");
                ImGui.Text("Start and end with '/' to search using regex.");
                ImGui.SameLine();
                ImGui.TextDisabled("\"/^.M.$/\"");


                ImGui.EndTooltip();
            }
        }

        public override string ToString() {
            return searchText;
        }
    }
}
