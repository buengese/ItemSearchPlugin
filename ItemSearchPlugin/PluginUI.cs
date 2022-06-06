using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;

namespace ItemSearchPlugin;

public class PluginUI : IDisposable
{
    internal ItemSearchPlugin Plugin { get; }
    
    private readonly Dictionary<ushort, TextureWrap> textureDictionary = new();

    private ItemSearchConfigWindow ConfigWindow { get; }
    internal ItemSearchWindow MainWindow { get; }

    internal PluginUI(ItemSearchPlugin plugin)
    {
        this.Plugin = plugin;

        this.MainWindow = new ItemSearchWindow(this);
        this.ConfigWindow = new ItemSearchConfigWindow(this);

        Service.PluginInterface.UiBuilder.Draw += this.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUI;
    }

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUI;
        Service.PluginInterface.UiBuilder.Draw -= this.Draw;
        
        this.MainWindow?.Dispose();
        
        foreach (var t in textureDictionary) {
            t.Value?.Dispose();
        }
        textureDictionary.Clear();
    }

    internal void ToggleMainUI()
    {
        this.MainWindow.Toggle();
    }
    
    internal void OpenConfigUI()
    {
        this.ConfigWindow.Open();
    }

    internal void ToggleConfigUI()
    {
        this.ConfigWindow.Toggle();
    }

    private void Draw()
    {
        this.MainWindow.Draw();
        this.ConfigWindow.Draw();
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
}