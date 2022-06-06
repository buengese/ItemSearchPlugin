using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using ItemSearchPlugin.DataSites;
using Lumina.Excel.GeneratedSheets;

namespace ItemSearchPlugin {
    public class ItemSearchPlugin : IDalamudPlugin {
        public string Name => "Item Search";

        internal TryOn TryOn { get; } = null;

        private PluginUI PluginUI { get; }

        internal CraftingRecipeFinder CraftingRecipeFinder { get; } = null;
        
        public static DataSite[] DataSites { get; private set; } = { new GarlandToolsDataSite() }; 
        public string Version { get; }
        
        public ItemSearchPlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            
            Service.Configuration = (ItemSearchPluginConfig) pluginInterface.GetPluginConfig() ?? new ItemSearchPluginConfig();
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            DataSites = new DataSite[] {
                new GarlandToolsDataSite(),
                new TeamcraftDataSite(),
                new GamerEscapeDatasite(),
            };
            
            Service.Configuration.ReloadLocalization();

            try
            {
                var address = new AddressResolver();
                address.Setup(Service.SigScanner);

                var gameFunctions = new GameFunctions(address);
                TryOn = new TryOn(gameFunctions);
                CraftingRecipeFinder = new CraftingRecipeFinder(address);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "GameFunction setup failed some feature disabled");
            }

            this.PluginUI = new PluginUI(this);
            
            SetupCommands();

#if DEBUG
            OnItemSearchCommand("", "");
#endif
        }

        public void Dispose() {
            PluginUI?.Dispose();
            CraftingRecipeFinder?.Dispose();
            TryOn?.Dispose();
            RemoveCommands();
        }


        private void SetupCommands() {
            Service.CommandManager.AddHandler("/xlitem", new Dalamud.Game.Command.CommandInfo(OnItemSearchCommand) {
                HelpMessage = Loc.Localize("ItemSearchCommandHelp", "Open a window you can use to link any specific item to chat."),
                ShowInHelp = true
            });
        }

        private void OnItemSearchCommand(string command, string args) {
            PluginUI.ToggleMainUI();
        }

        private void RemoveCommands() {
            Service.CommandManager.RemoveHandler("/xlitem");
#if DEBUG
            CommandManager.RemoveHandler("/itemsearchdumploc");
#endif
        }

    }
}
