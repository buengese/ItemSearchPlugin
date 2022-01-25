using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Hooking;
using Dalamud.Logging;

namespace ItemSearchPlugin {
    public unsafe class FittingRoomUI : IDisposable {

        private readonly ItemSearchPlugin plugin;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte TryOnDelegate(uint unknownCanEquip, uint itemBaseId, ulong stainColor, uint itemGlamourId, byte unknownByte);

        private readonly TryOnDelegate tryOn;

        private delegate IntPtr GetFittingRoomArrayLocation(IntPtr fittingRoomBaseAddress, byte unknownByte1, byte unknownByte2);
        private delegate void UpdateCharacterPreview(IntPtr a1, uint a2);

        private readonly Hook<GetFittingRoomArrayLocation> getFittingLocationHook;
        private Hook<UpdateCharacterPreview> updateCharacterPreviewHook;

        private IntPtr fittingRoomBaseAddress = IntPtr.Zero;

        private int tryOnDelay = 10;

        private readonly Queue<(uint itemid, uint stain)> tryOnQueue = new Queue<(uint itemid, uint stain)>();
        
        private readonly AddressResolver address;

        private delegate IntPtr GetInventoryContainer(IntPtr inventoryManager, int inventoryId);
        private delegate IntPtr GetContainerSlot(IntPtr inventoryContainer, int slotId);

        private enum TryOnControlID : uint {
            SetSaveDeleteButton = uint.MaxValue - 10,
            SuppressLog,
        }

        public FittingRoomUI(ItemSearchPlugin plugin) {
            this.plugin = plugin;

            try {
                address = new AddressResolver();
                address.Setup(ItemSearchPlugin.SigScanner);
                tryOn = Marshal.GetDelegateForFunctionPointer<TryOnDelegate>(address.TryOn);
                
                getFittingLocationHook = new Hook<GetFittingRoomArrayLocation>(address.GetTryOnArrayLocation, new GetFittingRoomArrayLocation(GetFittingRoomArrayLocationDetour));
                getFittingLocationHook.Enable();


                byte previewHookCounter = 0;
                updateCharacterPreviewHook = new Hook<UpdateCharacterPreview>(address.UpdateCharacterPreview, new UpdateCharacterPreview((a1, a2) => {
                    var visibleFlag = *(uint*) (a1 + 8);
                    var previewId = *(uint*) (a1 + 16);
                    if (visibleFlag == 5 && previewId == 2) {
                        // Visible and Fitting Room
                        fittingRoomBaseAddress = a1;
                        updateCharacterPreviewHook?.Original(a1, a2);
                        updateCharacterPreviewHook?.Disable();
                        updateCharacterPreviewHook?.Dispose();
                        updateCharacterPreviewHook = null;
                        return;
                    }

                    updateCharacterPreviewHook?.Original(a1, a2);
                    if (previewHookCounter++ <= 10) return;
                    // Fitting room probably isn't open, so can stop checking
                    updateCharacterPreviewHook?.Disable();
                    updateCharacterPreviewHook?.Dispose();
                    updateCharacterPreviewHook = null;
                }));

                updateCharacterPreviewHook.Enable();

                CanUseTryOn = true;
            } catch (Exception ex) {
                PluginLog.LogError(ex.ToString());
            }
        }

        private IntPtr GetFittingRoomArrayLocationDetour(IntPtr fittingRoomBaseAddress, byte unknownByte1, byte unknownByte2) {
            if (unknownByte1 == 0 && unknownByte2 == 1) {
                this.fittingRoomBaseAddress = fittingRoomBaseAddress;
                updateCharacterPreviewHook?.Disable();
                updateCharacterPreviewHook?.Dispose();
                updateCharacterPreviewHook = null;
            }

            return getFittingLocationHook.Original(fittingRoomBaseAddress, unknownByte1, unknownByte2);
        }

        public bool CanUseTryOn { get; }

        public void TryOnItem(Item item, uint stain = 0, bool hq = false) {
            #if DEBUG
            PluginLog.Log($"Try On: {item.Name}");
            #endif
            if (item.EquipSlotCategory.Row > 0 && item.EquipSlotCategory.Row != 6 && item.EquipSlotCategory.Row != 17 && (item.EquipSlotCategory.Value.OffHand <=0 || item.ItemUICategory.Row == 11)) {
                if (plugin.PluginConfig.SuppressTryOnMessage) tryOnQueue.Enqueue(((uint) TryOnControlID.SuppressLog, 1));
                tryOnQueue.Enqueue((item.RowId + (uint) (hq ? 1000000 : 0), stain));
                if (plugin.PluginConfig.SuppressTryOnMessage) tryOnQueue.Enqueue(((uint)TryOnControlID.SuppressLog, 0));
            }
#if DEBUG
            else {
                PluginLog.Log($"Cancelled Try On: Invalid Item. ({item.EquipSlotCategory.Row}, {item.EquipSlotCategory.Value.OffHand}, {item.EquipSlotCategory.Value.Waist}, {item.EquipSlotCategory.Value.SoulCrystal})");
            }
#endif
        }

        public void SetSaveDeleteButton(bool enabled) {
            if (fittingRoomBaseAddress != IntPtr.Zero) {
                Marshal.WriteByte(fittingRoomBaseAddress, 0x2BA, enabled ? (byte) 1 : (byte) 0);
            }
        }
        
        public void Draw() {
            
            while (CanUseTryOn && tryOnQueue.Count > 0 && (tryOnDelay <= 0 || tryOnDelay-- <= 0)) {
                try {
                    var (itemId, stain) = tryOnQueue.Dequeue();

                    switch ((TryOnControlID) itemId) {
                        case TryOnControlID.SetSaveDeleteButton: {
                            SetSaveDeleteButton(stain == 1);
                            break;
                        }
                        case TryOnControlID.SuppressLog: {
                            if (stain == 1) {
                                ItemSearchPlugin.Chat.ChatMessage += ChatOnOnChatMessage;
                            } else {
                                ItemSearchPlugin.Chat.ChatMessage -= ChatOnOnChatMessage;
                            }
                            break;
                        }
                        default: {
                            tryOnDelay = 1;
                            tryOn(0xFF, itemId, stain, 0, 0);
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
            if (type == XivChatType.SystemMessage && message.Payloads.Count > 1 && (ItemSearchPlugin.ClientState.ClientLanguage == ClientLanguage.Japanese ? message.Payloads[message.Payloads.Count - 1] : message.Payloads[0]) is TextPayload a) {

                bool handle = ItemSearchPlugin.ClientState.ClientLanguage switch {
                    ClientLanguage.English => a.Text.StartsWith("You try on "),
                    ClientLanguage.German => a.Text.StartsWith("Da hast "),
                    ClientLanguage.French => a.Text.StartsWith("Vous essayez "),
                    ClientLanguage.Japanese => a.Text.EndsWith("を試着した。"),
                    _ => false,
                };

                if (handle) {
                    isHandled = true;
                }
            }
        }

        public void Dispose() {
            getFittingLocationHook?.Disable();
            updateCharacterPreviewHook?.Disable();
        }

    }
}
