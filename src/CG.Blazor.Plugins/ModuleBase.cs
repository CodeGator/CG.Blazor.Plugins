
namespace CG.Blazor.Plugins
{
    /// <summary>
    /// This class is a base implementation of the <see cref="IModule"/> interface.
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <inheritdoc />
        public abstract void ConfigureServices(
            WebApplicationBuilder webApplicationBuilder,
            IConfiguration configuration,
            ILogger? bootstrapLogger
            );

        /// <inheritdoc />
        public abstract void Configure(
            WebApplication webApplication
            );

        #endregion
    }
}
