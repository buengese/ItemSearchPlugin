using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin;
using System;
using System.Dynamic;

namespace ItemSearch2.ActionButtons {
    class MarketBoardActionButton : IActionButton {
        
        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public override string GetButtonText(Item selectedItem) {
            return Loc.Localize("ItemSearchMarketButton", "Market");
        }

        public override bool GetShowButton(Item selectedItem) {
            return Service.Configuration.MarketBoardPluginIntegration && selectedItem.ItemSearchCategory.Row > 0 &&
                   Service.PluginInterface.PluginInternalNames.Contains("MarketBoardPlugin");
        }

        public override void OnButtonClicked(Item selectedItem) {
            Service.CommandManager.ProcessCommand($"/pmb {selectedItem.RowId}");
        }

        public override void Dispose() { }
    }
}
