
namespace CG.Blazor.Plugins.Options;

/// <summary>
/// This class represents configuration options for a Blazor plugin module.
/// </summary>
public class BlazorModuleOptions 
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property indicates whether the plugin is disabled, or not.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// This property contains an optional assembly name, or path, for the 
    /// Blazor plugin assembly that corresponds to the plugin.
    /// </summary>
    [Required]
    public string AssemblyNameOrPath { get; set; } = null!;

    /// <summary>
    /// This property indicates that the plugin requires routing support,
    /// from Blazor, at runtime.
    /// </summary>
    public bool IsRouted { get; set; }

    /// <summary>
    /// This property contains an optional list of resources, from the plugin, 
    /// that should be injected into the HTML head section, at runtime.
    /// </summary>
    public List<string> StyleSheets { get; set; } = new();

    /// <summary>
    /// This property contains an optional list of scripts, from the plugin, 
    /// that should be injected into the HTML head section, at runtime.
    /// </summary>
    public List<string> Scripts { get; set; } = new();

    /// <summary>
    /// This property contains the optional name of an options entry point 
    /// for the plugin.
    /// </summary>
    public string? EntryPoint { get; set; }

    #endregion
}
