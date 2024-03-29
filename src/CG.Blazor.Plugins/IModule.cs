﻿
namespace CG.Blazor.Plugins;

/// <summary>
/// This interface represents a Blazor plugin module.
/// </summary>
public interface IModule
{
    /// <summary>
    /// This method is called by the framework when the module is first 
    /// loaded, to configure the services within the plugin. 
    /// </summary>
    /// <param name="webApplicationBuilder">The web application to use 
    /// for the operation.</param>
    /// <param name="configuration">The configuration section to use for 
    /// the operation.</param>
    /// <param name="bootstrapLogger">An optional bootstrap logger to 
    /// use for the operation.</param>
    /// <remarks>
    /// <para>
    /// The <paramref name="configuration"/> parameter is isolated to the 
    /// current module's configuration settings so that each module can then
    /// add whatever configuration settings are required, for that module. So,
    /// for instance, if a module requires repository options, the section
    /// can be conveniently placed inside the module's configuration section.
    /// </para>
    /// </remarks>
    void ConfigureServices(
        WebApplicationBuilder webApplicationBuilder,
        IConfiguration configuration,
        ILogger? bootstrapLogger
        );

    /// <summary>
    /// This method is called by the framework when the module is first
    /// loaded, to configure the logic within the plugin
    /// </summary>
    /// <param name="webApplication">The web application to use for the 
    /// operation.</param>
    void Configure(
        WebApplication webApplication
        );
}
