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

        internal CraftingRecipeFinder CraftingRecipeFinder { get; } = null;

        private readonly Dictionary<ushort, TextureWrap> textureDictionary = new();

        internal ItemSearchWindow itemSearchWindow;
        
        private bool drawItemSearchWindow;

        private bool drawConfigWindow;

        internal List<GenericItem> LuminaItems { get; set; }
        internal ClientLanguage LuminaItemsClientLanguage { get; set; }
        
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


            Service.PluginInterface.UiBuilder.Draw += this.BuildUI;
            SetupCommands();

#if DEBUG
            OnItemSearchCommand("", "");
#endif
        }

        public void Dispose() {
            Service.PluginInterface.UiBuilder.Draw -= this.BuildUI;
            CraftingRecipeFinder?.Dispose();
            itemSearchWindow?.Dispose();
            TryOn?.Dispose();
            RemoveCommands();
            

            foreach (var t in textureDictionary) {
                t.Value?.Dispose();
            }

            textureDictionary.Clear();
        }


        private void SetupCommands() {
            Service.CommandManager.AddHandler("/xlitem", new Dalamud.Game.Command.CommandInfo(OnItemSearchCommand) {
                HelpMessage = Loc.Localize("ItemSearchCommandHelp", "Open a window you can use to link any specific item to chat."),
                ShowInHelp = true
            });
        }

        private void OnItemSearchCommand(string command, string args) {
            itemSearchWindow?.Dispose();
            itemSearchWindow = new ItemSearchWindow(this, args);
            drawItemSearchWindow = true;
        }

        private void RemoveCommands() {
            Service.CommandManager.RemoveHandler("/xlitem");
#if DEBUG
            CommandManager.RemoveHandler("/itemsearchdumploc");
#endif
        }
        
        private void BuildUI() {
            if (drawItemSearchWindow) {

                drawItemSearchWindow = itemSearchWindow != null && itemSearchWindow.Draw();
                drawConfigWindow = drawItemSearchWindow && drawConfigWindow && Service.Configuration.DrawConfigUi();

                if (drawItemSearchWindow == false) {
                    itemSearchWindow?.Dispose();
                    itemSearchWindow = null;
                    drawConfigWindow = false;
                }
            }
        }

        internal void LinkItem(GenericItem item) {
            if (item == null) {
                PluginLog.Log("Tried to link NULL item.");
                return;
            }

            var payloadList = new List<Payload> {
                new UIForegroundPayload((ushort) (0x223 + item.Rarity * 2)),
                new UIGlowPayload((ushort) (0x224 + item.Rarity * 2)),
                new ItemPayload(item.RowId, item.CanBeHq && Service.KeyState[0x11]),
                new UIForegroundPayload(500),
                new UIGlowPayload(501),
                new TextPayload($"{(char) SeIconChar.LinkMarker}"),
                new UIForegroundPayload(0),
                new UIGlowPayload(0),
                new TextPayload(item.Name + (item.CanBeHq && Service.KeyState[0x11] ? $" {(char)SeIconChar.HighQuality}" : "")),
                new RawPayload(new byte[] {0x02, 0x27, 0x07, 0xCF, 0x01, 0x01, 0x01, 0xFF, 0x01, 0x03}),
                new RawPayload(new byte[] {0x02, 0x13, 0x02, 0xEC, 0x03})
            };

            var payload = new SeString(payloadList);

            Service.Chat.PrintChat(new XivChatEntry {
                Message = payload
            });
        }

        internal void DrawIcon(ushort icon, Vector2 size) {
            if (icon < 65000) {
                if (textureDictionary.ContainsKey(icon)) {
                    var tex = textureDictionary[icon];
                    if (tex == null || tex.ImGuiHandle == IntPtr.Zero) {
                        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                        ImGui.BeginChild("FailedTexture", size, true);
                        ImGui.Text(icon.ToString());
                        ImGui.EndChild();
                        ImGui.PopStyleColor();
                    } else {
                        ImGui.Image(textureDictionary[icon].ImGuiHandle, size);
                    }
                } else {
                    ImGui.BeginChild("WaitingTexture", size, true);
                    ImGui.EndChild();

                    textureDictionary[icon] = null;

                    Task.Run(() => {
                        try {
                            var iconTex = Service.Data.GetIcon(icon);
                            var tex = Service.PluginInterface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex != null && tex.ImGuiHandle != IntPtr.Zero) {
                                textureDictionary[icon] = tex;
                            }
                        } catch {
                            // Ignore
                        }
                    });
                }
            } else {
                ImGui.BeginChild("NoIcon", size, true);
                if (Service.Configuration.ShowItemID) {
                    ImGui.Text(icon.ToString());
                }

                ImGui.EndChild();
            }
        }

        internal void ToggleConfigWindow() {
            drawConfigWindow = !drawConfigWindow;
        }

    }
}
