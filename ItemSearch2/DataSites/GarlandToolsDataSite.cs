using Lumina.Excel.GeneratedSheets;

namespace ItemSearch2.DataSites {
    public class GarlandToolsDataSite : DataSite {
        public override string Name => "Garland Tools";

        public override string NameTranslationKey => "GarlandToolsDataSite";

        public override string GetItemUrl(Item item) => $"https://www.garlandtools.org/db/#item/{item.RowId}";
    }
}
