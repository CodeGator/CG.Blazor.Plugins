
namespace CG.Blazor.Plugins.TestPlugin;

/// <summary>
/// This class represents the plugin module's startup logic.
/// </summary>
public class Module : ModuleBase
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <inheritdoc/>
    public override void ConfigureServices(
        WebApplicationBuilder webApplicationBuilder,
        IConfiguration configuration
        )
    {
        // TODO : add your plugin's registration logic here.
    }

    // *******************************************************************

    /// <inheritdoc/>
    public override void Configure(
        WebApplication webApplication
        )
    {
        // TODO : add your plugin's startup logic here.
    }

    #endregion
}
