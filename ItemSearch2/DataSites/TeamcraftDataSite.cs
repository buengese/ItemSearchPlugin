using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearch2.DataSites {
    public class TeamcraftDataSite : DataSite {
        public override string Name => "Teamcraft";

        public override string NameTranslationKey => "TeamcraftDataSite";

        public override string GetItemUrl(Item item) => $"https://ffxivteamcraft.com/db/en/item/{item.RowId}/{item.Name.ToString().Replace(' ', '-')}";
        
    }
}
