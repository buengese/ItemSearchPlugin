using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud;
using ItemSearch2.ActionButtons;

namespace ItemSearch2 {

    public class ItemSearchPluginConfig : IPluginConfiguration {
        [JsonIgnore] internal List<(string localizationKey, string englishName)> FilterNames { get; } = new List<(string localizationKey, string englishName)>();

        public int Version { get; set; }

        private string Language { get; set; }

        public bool CloseOnChoose { get; set; }

        public bool ShowItemID { get; set; }

        public bool ShowTryOn { get; set; }

        public string DataSite { get; set; }

        public SortedSet<uint> Favorites { get; set; } = new SortedSet<uint>();
        
        public bool MarketBoardPluginIntegration { get; set; }
        
        public bool ShowLegacyItems { get; set; }

        public byte SelectedLanguage { get; set; }

        public bool PrependFilterListWithCopy { get; set; }
        public List<string> DisabledFilters { get; set; }
        
        [NonSerialized] private DataSite lastDataSite;

        public uint SelectedStain { get; set; } = 0;

        public bool ExpandedFilters { get; set; } = false;

        [JsonIgnore]
        public DataSite SelectedDataSite {
            get {
                if (lastDataSite == null || (lastDataSite.Name != this.DataSite)) {
                    if (string.IsNullOrEmpty(this.DataSite)) {
                        return null;
                    }

                    lastDataSite = ItemSearchPlugin.DataSites.FirstOrDefault(ds => ds.Name == this.DataSite);
                }

                return lastDataSite;
            }
        }

        [JsonIgnore]
        public ClientLanguage SelectedClientLanguage {
            get {
                return SelectedLanguage switch {
                    0 => Service.ClientState.ClientLanguage,
                    1 => ClientLanguage.English,
                    2 => ClientLanguage.Japanese,
                    3 => ClientLanguage.French,
                    4 => ClientLanguage.German,
                    _ => Service.ClientState.ClientLanguage,
                };
            }
        }

        public bool HideKofi { get; set; } = false;
        public bool TryOnEnabled { get; set; } = false;
        public bool AutoFocus { get; set; } = true;
        public bool SuppressTryOnMessage { get; set; } = true;
        public bool EnableFFXIVStore { get; set; } = false;

        public ItemSearchPluginConfig()
        {
            LoadDefaults();
        }

        private void LoadDefaults() {
            CloseOnChoose = false;
            ShowItemID = false;
            MarketBoardPluginIntegration = false;
            ShowTryOn = false;
            SuppressTryOnMessage = true;
            ShowLegacyItems = false;
            DataSite = ItemSearchPlugin.DataSites.FirstOrDefault()?.Name;
            SelectedLanguage = 0;
            DisabledFilters = new List<string>();
            PrependFilterListWithCopy = false;
            AutoFocus = true;
            HideKofi = false;
        }

        public void Save() {
            Service.PluginInterface.SavePluginConfig(this);
        }
        
        internal void ReloadLocalization() {
            if (!string.IsNullOrEmpty(Language)) {
                Loc.LoadLanguage(Language);
            } else {
                Loc.LoadDefaultLanguage();
            }
        }
        
    }
}
