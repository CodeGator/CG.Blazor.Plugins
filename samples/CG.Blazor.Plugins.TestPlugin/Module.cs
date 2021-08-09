using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CG.Blazor.Plugins.TestPlugin
{
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
            IServiceCollection serviceCollection,
            IConfiguration configuration
            )
        {
            // TODO : add your plugin's registration logic here.
        }

        // *******************************************************************

        /// <inheritdoc/>
        public override void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env
            )
        {
            // TODO : add your plugin's startup logic here.
        }

        #endregion
    }
}
