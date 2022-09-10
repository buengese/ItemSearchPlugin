using System;
using System.Reflection;
using Dalamud.Logging;
using Dalamud.Plugin;
using ItemSearch2.DataSites;

namespace ItemSearch2 {
    public class ItemSearchPlugin : IDalamudPlugin {
        public string Name => "Item Search";

        internal TryOn TryOn { get; } = null;

        private PluginUI PluginUI { get; }

        private CraftingRecipeFinder CraftingRecipeFinder { get; } = null;
        
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
                CraftingRecipeFinder = new CraftingRecipeFinder();
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
        }

    }
}
