using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin.ActionButtons {
    class DataSiteActionButton : IActionButton {

        public override ActionButtonPosition ButtonPosition => ActionButtonPosition.TOP;

        public override void Dispose() { }

        public override string GetButtonText(Item selectedItem) {
            return string.Format(
                Loc.Localize("ItemSearchDataSiteViewButton", "View on {0}"),
                Loc.Localize(Service.Configuration.SelectedDataSite.NameTranslationKey, Service.Configuration.SelectedDataSite.Name)
            );
        }

        public override bool GetShowButton(Item selectedItem) {
            return Service.Configuration.SelectedDataSite != null;
        }

        public override void OnButtonClicked(Item selectedItem) {
            Service.Configuration.SelectedDataSite.OpenItem(selectedItem);
        }
    }
}
