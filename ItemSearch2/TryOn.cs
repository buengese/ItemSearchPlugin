using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Game;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Logging;

namespace ItemSearch2 {
    public class TryOn : IDisposable {
        
        private GameFunctions GameFunctions { get; }
        
        private int tryOnDelay = 10;

        private readonly Queue<(uint itemid, uint stain)> tryOnQueue = new();

        private enum TryOnControlID : uint {
            SuppressLog = uint.MaxValue - 10,
        }

        public TryOn(GameFunctions gameFunctions)
        {
            this.GameFunctions = gameFunctions;
            Service.Framework.Update += FrameworkUpdate;
        }
        
        public void TryOnItem(Item item, uint stain = 0, bool hq = false) {
#if DEBUG
            PluginLog.Log($"Try On: {item.Name}");
#endif
            if (item.EquipSlotCategory?.Value == null) return;
            if (item.EquipSlotCategory.Row > 0 && item.EquipSlotCategory.Row != 6 && item.EquipSlotCategory.Row != 17 && (item.EquipSlotCategory.Value.OffHand <=0 || item.ItemUICategory.Row == 11)) {
                if (Service.Configuration.SuppressTryOnMessage) tryOnQueue.Enqueue(((uint) TryOnControlID.SuppressLog, 1));
                tryOnQueue.Enqueue((item.RowId + (uint) (hq ? 1000000 : 0), stain));
                if (Service.Configuration.SuppressTryOnMessage) tryOnQueue.Enqueue(((uint)TryOnControlID.SuppressLog, 0));
            }
#if DEBUG
            else {
                PluginLog.Log($"Cancelled Try On: Invalid Item. ({item.EquipSlotCategory.Row}, {item.EquipSlotCategory.Value.OffHand}, {item.EquipSlotCategory.Value.Waist}, {item.EquipSlotCategory.Value.SoulCrystal})");
            }
#endif
        }

        private void FrameworkUpdate(Framework framework) {
            
            while (tryOnQueue.Count > 0 && (tryOnDelay <= 0 || tryOnDelay-- <= 0)) {
                try {
                    var (itemId, stain) = tryOnQueue.Dequeue();

                    switch ((TryOnControlID) itemId) {
                        case TryOnControlID.SuppressLog: {
                            if (stain == 1) {
                                Service.Chat.ChatMessage += ChatOnOnChatMessage;
                            } else {
                                Service.Chat.ChatMessage -= ChatOnOnChatMessage;
                            }
                            break;
                        }
                        default: {
                            tryOnDelay = 1;
                            GameFunctions._tryOn(0xFF, itemId, stain, 0, 0);
                            break;
                        }
                    }

                } catch {
                    tryOnDelay = 5;
                    break;
                }
            }
        }

        private void ChatOnOnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled) {
            if (type != XivChatType.SystemMessage || message.Payloads.Count <= 1 ||
                (Service.ClientState.ClientLanguage == ClientLanguage.Japanese ? message.Payloads[message.Payloads.Count - 1] : message.Payloads[0]) 
                is not TextPayload a) return;
            var handle = Service.ClientState.ClientLanguage switch {
                ClientLanguage.English => a.Text?.StartsWith("You try on ") ?? false,
                ClientLanguage.German => a.Text?.StartsWith("Da hast ") ?? false,
                ClientLanguage.French => a.Text?.StartsWith("Vous essayez ") ?? false,
                ClientLanguage.Japanese => a.Text?.EndsWith("を試着した。") ?? false,
                _ => false,
            };
            if (handle) isHandled = true;
        }

        public void Dispose() {
            Service.Framework.Update -= FrameworkUpdate;
        }
    }
}