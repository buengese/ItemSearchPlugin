using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using ItemSearchPlugin.ActionButtons;

namespace ItemSearchPlugin;

public class ItemSearchConfigWindow
{
    private bool _visible;
    private PluginUI PluginUI { get; }
    
    public ItemSearchConfigWindow(PluginUI pluginUI)
    {
        this.PluginUI = pluginUI;
    }

    internal void Open()
    {
        this._visible = true;
    }

    internal void Toggle()
    {
        this._visible ^= true;
    }


    public void Draw() {
        if (!this._visible)
        {
            return;
        }

        ImGui.Begin("Item Search Config", ref this._visible, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse);
        

        int selectedLanguage = Service.Configuration.SelectedLanguage;
        var langName = selectedLanguage == 0
            ? Loc.Localize("LanguageAutomatic", "Automatic")
            : Service.Configuration.SelectedClientLanguage.ToString();
        if (ImGui.BeginCombo(Loc.Localize("ItemSearchConfigItemLanguage", "Item Language") + "###ItemSearchConfigLanguageSelect", 
                langName)) {
            if (ImGui.Selectable(Loc.Localize("LanguageAutomatic", "Automatic"), selectedLanguage == 0)) selectedLanguage = 0;
            if (ImGui.Selectable("English##itemLanguageOption", selectedLanguage == 1)) selectedLanguage = 1;
            if (ImGui.Selectable("日本語##itemLanguageOption", selectedLanguage == 2)) selectedLanguage = 2;
            if (ImGui.Selectable("Français##itemLanguageOption", selectedLanguage == 3)) selectedLanguage = 3;
            if (ImGui.Selectable("Deutsch##itemLanguageOption", selectedLanguage == 4)) selectedLanguage = 4;
            if (Service.Configuration.SelectedLanguage != selectedLanguage)
            {
                Service.Configuration.SelectedLanguage = (byte)selectedLanguage;
                Service.Configuration.Save();
            }

            ImGui.EndCombo();
        }

        bool closeOnChoose = Service.Configuration.CloseOnChoose;
        if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigCloseAfterLink", "Close window after linking item"), ref closeOnChoose)) {
            Service.Configuration.CloseOnChoose = closeOnChoose;
            Service.Configuration.Save();
        }

        bool autoFocus = Service.Configuration.AutoFocus;
        if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigAutoFocus", "Auto focus search box"), ref autoFocus)) {
            Service.Configuration.AutoFocus = autoFocus;
            Service.Configuration.Save();
        }

        bool showItemId = Service.Configuration.ShowItemID;
        if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigShowItemId", "Show Item IDs"), ref showItemId)) {
            Service.Configuration.ShowItemID = showItemId;
            Service.Configuration.Save();
        }

        bool mbpIntegration = Service.Configuration.MarketBoardPluginIntegration;
        if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigEnableMarketBoard", "Market Board Plugin Integration"), ref mbpIntegration)) {
            Service.Configuration.MarketBoardPluginIntegration = mbpIntegration;
            Service.Configuration.Save();
        }

        bool showTryOn = Service.Configuration.ShowTryOn;
        if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigEnableTryOn", "Enable Try On Feature"), ref showTryOn)) {
            Service.Configuration.ShowTryOn = showTryOn;
            Service.Configuration.Save();
        }

        bool suppressTryOnMessage = Service.Configuration.SuppressTryOnMessage;
        if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigSuppressTryOnMessage", "Surppress Try On Message"), ref suppressTryOnMessage)) {
            Service.Configuration.SuppressTryOnMessage = suppressTryOnMessage;
            Service.Configuration.Save();
        }

        var prependFilterListWithCopy = Service.Configuration.PrependFilterListWithCopy;
        if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigPrependFilterListWithCopy", "Add filters when copying results to clipboard"), ref prependFilterListWithCopy)) {
            Service.Configuration.PrependFilterListWithCopy = prependFilterListWithCopy;
            Service.Configuration.Save();
        }

        bool showLegacyItems = Service.Configuration.ShowLegacyItems;
        if (ImGui.Checkbox(Loc.Localize("ItemSearchConfigShowLegacyItems", "Show Legacy Items"), ref showLegacyItems)) {
            Service.Configuration.ShowLegacyItems = showLegacyItems;
            Service.Configuration.Save();
        }

        bool hideKofi = Service.Configuration.HideKofi;
        if (ImGui.Checkbox(Loc.Localize("HideKofi", "Don't show Ko-fi link"), ref hideKofi)) {
            Service.Configuration.HideKofi = hideKofi;
            Service.Configuration.Save();
        }

        int dataSiteIndex = Array.IndexOf(ItemSearchPlugin.DataSites, Service.Configuration.SelectedDataSite);
        if (ImGui.Combo(Loc.Localize("ItemSearchConfigExternalDataSite", "External Data Site"), ref dataSiteIndex, 
                ItemSearchPlugin.DataSites.Select(t => Loc.Localize(t.NameTranslationKey, t.Name) + (string.IsNullOrEmpty(t.Note) ? "" : "*")).ToArray(), ItemSearchPlugin.DataSites.Length)) {
            Service.Configuration.DataSite = ItemSearchPlugin.DataSites[dataSiteIndex].Name;
            Service.Configuration.Save();
        }

        if (!string.IsNullOrEmpty(Service.Configuration.SelectedDataSite.Note)) {
            ImGui.TextColored(new Vector4(1, 1, 1, 0.5f), $"*{Service.Configuration.SelectedDataSite.Note}");
        }

        var storeEnabled = Service.Configuration.EnableFFXIVStore;
        if (ImGui.Checkbox("FFXIV Store", ref storeEnabled)) {
            Service.Configuration.EnableFFXIVStore = storeEnabled;
            FfxivStoreActionButton.BeginUpdate();
            Service.Configuration.Save();
        }

        if (!storeEnabled && ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Warning: FFXIV Store");
            ImGui.Separator();
            ImGui.TextWrapped("Enabling FFXIV Store will cause Item Search Plugin to contact the FFXIV Store website to determine which items are available.");
            ImGui.Text("If this concerns you, you probably shouldn't enable it.");
            ImGui.EndTooltip();
        }

        ImGui.SameLine();
        ImGui.TextDisabled($"{FfxivStoreActionButton.UpdateStatus}");

        ImGui.Text("Show Filters: ");

        ImGui.BeginChild("###scrollingFilterSelection", new Vector2(0, 180), true);

        ImGui.Columns(2, "###itemSearchToggleFilters", false);
        foreach (var (localizationKey, englishName) in Service.Configuration.FilterNames) {
            var enabled = !Service.Configuration.DisabledFilters.Contains(localizationKey);
            if (ImGui.Checkbox(Loc.Localize(localizationKey, englishName) + "##checkboxToggleFilterEnabled", ref enabled)) {
                if (enabled)
                {
                    Service.Configuration.DisabledFilters.RemoveAll(a => a == localizationKey);
                    PluginUI.MainWindow.SearchFilters.FirstOrDefault(f => f.NameLocalizationKey == localizationKey)
                        ?.Show();
                }
                else
                { 
                    Service.Configuration.DisabledFilters.Add(localizationKey);
                    PluginUI.MainWindow.SearchFilters.FirstOrDefault(f => f.NameLocalizationKey == localizationKey)?.Hide();
                }

                Service.Configuration.Save();
            }

            ImGui.NextColumn();
        }

        ImGui.Columns(1);
        ImGui.EndChild();

        ImGui.End();
    }
}