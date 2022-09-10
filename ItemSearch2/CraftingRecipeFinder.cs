using System;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Concurrent;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace ItemSearch2 {
    public class CraftingRecipeFinder : IDisposable {
        private readonly ConcurrentQueue<uint> searchQueue = new();

        private bool disposed;

        private unsafe void OnFrameworkUpdate(Framework framework) {
            try {
                if (disposed) return;
                if (Service.ClientState.LocalContentId == 0) return;
                if (!searchQueue.TryDequeue(out var itemID)) {
                    Service.Framework.Update -= OnFrameworkUpdate;
                    return;
                }

                AgentRecipeNote.Instance()->OpenRecipeByItemId(itemID);
            } catch (NullReferenceException) { }
        }

        public void SearchRecipesByItem(Item item) {
            if (disposed) return;
            if (item == null) {
                PluginLog.Log("Tried to find recipe for NULL item.");
                return;
            }

            searchQueue.Enqueue(item.RowId);
            Service.Framework.Update -= OnFrameworkUpdate;
            Service.Framework.Update += OnFrameworkUpdate;
        }

        public void Dispose() {
            disposed = true;
            Service.Framework.Update -= OnFrameworkUpdate;
        }
    }
}