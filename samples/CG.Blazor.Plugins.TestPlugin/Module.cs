
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
        IConfiguration configuration,
        ILogger? bootstrapLogger
        )
    {
        // TODO : add your plugin's registration / startup logic here.
    }

    // *******************************************************************

    /// <inheritdoc/>
    public override void Configure(
        WebApplication webApplication
        )
    {
        // TODO : add your plugin's startup / pipeline logic here.
    }

    #endregion
}
