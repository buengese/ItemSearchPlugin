﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace ItemSearch2.ActionButtons {
    class CopyItemAsJson : IActionButton {

        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public override void Dispose() { }

        public override string GetButtonText(Item selectedItem) {
            return Loc.Localize("ItemSearchCopyAsJson", "Copy Information");
        }

        public override bool GetShowButton(Item selectedItem) {
            #if DEBUG
            return true;
            #else
            return false;
            #endif
        }

        public override void OnButtonClicked(Item selectedItem) {
            
            var sb = new StringBuilder();

            foreach (var f in typeof(Item).GetFields()) {

                sb.AppendLine($"{f.Name}: {f.GetValue(selectedItem)}");
            }


            

            var recipes = Service.Data.GetExcelSheet<Recipe>()?.Where(a => a.ItemResult.Row == selectedItem.RowId).ToList() ?? new List<Recipe>();

            if (recipes.Count == 0) {
                sb.Append("Recipes: NONE");
            } else {
                sb.AppendLine("Recipes:");
                foreach (var r in recipes) {

                    sb.AppendLine($"  Recipe: {r.RowId}");
                    sb.AppendLine("    Ingredients:");
                    foreach (var ri in r.UnkData5) {

                        sb.AppendLine($"      [{ri.ItemIngredient}*{ri.AmountIngredient}] {Service.Data.GetExcelSheet<Item>()?.GetRow((uint) ri.ItemIngredient)?.Name} x {ri.AmountIngredient}");


                    } 
                    foreach (var rf in typeof(Recipe).GetFields()) {
                        sb.AppendLine($"    {rf.Name}: {rf.GetValue(r)}");
                    }
                }
            }







            ImGui.SetClipboardText(sb.ToString());


        }
    }
}
