using System.Collections.Generic;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;

namespace ItemSearchPlugin;

public class ChatHelper
{
    internal static void LinkItem(GenericItem item) {
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
}