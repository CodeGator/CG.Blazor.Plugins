
namespace CG.Blazor.Plugins.Options;

/// <summary>
/// This class represents configuration options for Blazor plugins.
/// </summary>
public class BlazorPluginsOptions
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property contains an optional list of plugin modules.
    /// </summary>
    public IList<BlazorModuleOptions> Modules { get; set; }

    #endregion

    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="BlazorPluginsOptions"/>
    /// class.
    /// </summary>
    public BlazorPluginsOptions()
    {
        // Make the compiler happy with default values.
        Modules = new List<BlazorModuleOptions>();
    }

    #endregion
}
