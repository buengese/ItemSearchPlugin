using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;

namespace ItemSearchPlugin;

public class PluginUI : IDisposable
{
    public ImFontPtr fontPtr;
    internal ItemSearchPlugin Plugin { get; }
    
    private readonly Dictionary<ushort, TextureWrap> textureDictionary = new();

    private readonly ItemSearchConfigWindow configWindow;
    internal readonly ItemSearchWindow MainWindow;

    internal PluginUI(ItemSearchPlugin plugin)
    {
        this.Plugin = plugin;

        this.MainWindow = new ItemSearchWindow(this);
        this.configWindow = new ItemSearchConfigWindow(this);

        Service.PluginInterface.UiBuilder.BuildFonts += this.HandleBuildFonts;
        
        Service.PluginInterface.UiBuilder.RebuildFonts();

        Service.PluginInterface.UiBuilder.Draw += this.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUI;

    }

    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.BuildFonts -= this.HandleBuildFonts;
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

    private void OpenConfigUI()
    {
        this.configWindow.Open();
    }

    internal void ToggleConfigUI()
    {
        this.configWindow.Toggle();
    }

    private void Draw()
    {
        this.MainWindow.Draw();
        this.configWindow.Draw();
    }
    
    private unsafe void HandleBuildFonts()
    {
        var fontPath = Path.Combine(Service.PluginInterface.DalamudAssetDirectory.FullName, "UIRes", "NotoSansCJKjp-Medium.otf");
        this.fontPtr = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 17.0f);

        ImFontConfigPtr fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
        fontConfig.MergeMode = true;
        fontConfig.NativePtr->DstFont = UiBuilder.DefaultFont.NativePtr;

        var fontRangeHandle = GCHandle.Alloc(
            new ushort[]
            {
                0x202F,
                0x202F,
                0,
            },
            GCHandleType.Pinned);

        if (Service.PluginInterface.AssemblyLocation.DirectoryName != null)
        {
            var otherPath = Path.Combine(Service.PluginInterface.AssemblyLocation.DirectoryName, "Resources", "NotoSans-Medium.otf");
            ImGui.GetIO().Fonts.AddFontFromFileTTF(otherPath, 17.0f, fontConfig, fontRangeHandle.AddrOfPinnedObject());
        }

        fontConfig.Destroy();
        fontRangeHandle.Free();
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