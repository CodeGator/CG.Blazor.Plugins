
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
    /// This property indicates whether the plugin is enabled for loading, or not.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// This property contains an optional assembly name, or path, for the 
    /// Blazor plugin assembly that corresponds to the plugin.
    /// </summary>
    public string AssemblyNameOrPath { get; set; }

    /// <summary>
    /// This property indicates that the plugin requires routing support,
    /// from Blazor, at runtime.
    /// </summary>
    public bool Routed { get; set; }

    /// <summary>
    /// This property contains an optional list of resources, from the plugin, 
    /// that should be injected into the HTML head section, at runtime.
    /// </summary>
    public IList<string> StyleSheets { get; set; }

    /// <summary>
    /// This property contains an optional list of scripts, from the plugin, 
    /// that should be injected into the HTML head section, at runtime.
    /// </summary>
    public IList<string> Scripts { get; set; }

    /// <summary>
    /// This property contains the name of an options entry point for the 
    /// plugin.
    /// </summary>
    public string EntryPoint { get; set; }

    #endregion

    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="BlazorModuleOptions"/>
    /// class.
    /// </summary>
    public BlazorModuleOptions()
    {
        // Make the compiler happy with default values.
        IsEnabled = true;
        AssemblyNameOrPath = "";
        Routed = false;
        StyleSheets = new List<string>();
        Scripts = new List<string>();
        EntryPoint = "";
    }

    #endregion
}
