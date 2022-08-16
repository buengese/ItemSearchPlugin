using Dalamud.Game;
using Dalamud.Game.Internal;
using System;
using Dalamud.Logging;

namespace ItemSearch2 {
    public class AddressResolver : BaseAddressResolver {
        public IntPtr TryOn { get; private set; }
        public IntPtr GetUiObject { get; private set; }
        public IntPtr GetAgentObject { get; private set; }
        public IntPtr SearchItemByCraftingMethod { get; private set; }
        public IntPtr CardUnlockedStatic { get; private set; }
        public IntPtr IsCardUnlocked { get; private set; }
        // public IntPtr IsItemActionUnlocked { get; private set; }

        protected override void Setup64Bit(SigScanner sig) {
            TryOn = sig.ScanText("E8 ?? ?? ?? ?? EB 35 BA ?? ?? ?? ??");
            GetUiObject = sig.ScanText("E8 ?? ?? ?? ?? 48 8B C8 48 8B 10 FF 52 40 80 88 ?? ?? ?? ?? 01 E9");
            GetAgentObject = sig.ScanText("E8 ?? ?? ?? ?? 83 FF 0D");
            SearchItemByCraftingMethod = sig.ScanText("E8 ?? ?? ?? ?? EB 7A 48 83 F8 06");
            
            CardUnlockedStatic = sig.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 45 33 C0 BA ?? ?? ?? ?? E8 ?? ?? ?? ?? 0F B6 93");
            IsCardUnlocked = sig.ScanText("E8 ?? ?? ?? ?? 8D 7B 78");
            // IsItemActionUnlocked = sig.ScanText("E8 ?? ?? ?? ?? 84 C0 75 A9");
            
            PluginLog.Verbose("===== X L I T E M =====");
            PluginLog.Verbose($"{nameof(this.TryOn)}   0x{this.TryOn:X}");
            PluginLog.Verbose($"{nameof(this.GetUiObject)} 0x{this.GetUiObject:X}");
            PluginLog.Verbose($"{nameof(this.GetAgentObject)}            0x{this.GetAgentObject:X}");
            PluginLog.Verbose($"{nameof(this.SearchItemByCraftingMethod)}         0x{this.SearchItemByCraftingMethod:X}");
            PluginLog.Verbose($"{nameof(this.CardUnlockedStatic)}         0x{this.CardUnlockedStatic:X}");
            PluginLog.Verbose($"{nameof(this.IsCardUnlocked)}         0x{this.IsCardUnlocked:X}");
            // PluginLog.Verbose($"{nameof(this.IsItemActionUnlocked)}         0x{this.IsItemActionUnlocked:X}");

        }
    }
}
