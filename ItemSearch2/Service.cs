using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace ItemSearch2;

public class Service
{
    /// <summary>
    /// Gets or sets the plugin configuration.
    /// </summary>
    internal static ItemSearchPluginConfig Configuration { get; set; } = null!;
    
    /// <summary>
    /// Gets the Dalamud plugin interface.
    /// </summary>
    [PluginService]
    public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    
    /// <summary>
    /// Gets the Dalamud chat gui.
    /// </summary>
    [PluginService]
    public static ChatGui Chat { get; private set; } = null!;
    
    /// <summary>
    /// Gets the Dalamud client state.
    /// </summary>
    [PluginService]
    public static ClientState ClientState { get; private set; } = null!;
    
    /// <summary>
    /// Gets the Dalamud command manager.
    /// </summary>
    [PluginService]
    public static CommandManager CommandManager { get; private set; } = null!;

    /// <summary>
    /// Gets the Dalamud data manager.
    /// </summary>
    [PluginService]
    public static DataManager Data { get; private set; } = null!;
    
    /// <summary>
    /// Gets the Dalamud framework manager.
    /// </summary>
    [PluginService]
    public static Framework Framework { get; private set; } = null!;

    /// <summary>
    /// Gets the Game gui manager.
    /// </summary>
    [PluginService]
    public static GameGui GameGui { get; private set; } = null!;

    [PluginService]
    public static KeyState KeyState { get; private set; } = null!;
    
    [PluginService]
    public static SigScanner SigScanner { get; private set; } = null!;
        

}