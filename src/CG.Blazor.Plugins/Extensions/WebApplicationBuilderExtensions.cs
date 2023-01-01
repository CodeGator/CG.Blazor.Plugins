
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// This class contains extension methods related to the <see cref="WebApplicationBuilder"/>
/// </summary>
public static class WebApplicationBuilderExtensions
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method adds plugins to the given web application builder. 
    /// </summary>
    /// <param name="webApplicationBuilder">The web application builder to 
    /// use for the operation.</param>
    /// <param name="configurationSection">The default configuration section
    /// to read options from. </param>
    /// <param name="bootstrapLogger">An optional bootstrap logger to use
    /// for the operation.</param>
    /// <returns>The value of the <paramref name="webApplicationBuilder"/>
    /// parameter, for chaining calls together, Fluent style.</returns>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments is missing, or invalid.</exception>
    /// <exception cref="BlazorPluginException">This exception is thrown 
    /// whenever the operation fails.</exception>
    public static WebApplicationBuilder AddBlazorPlugins(
        this WebApplicationBuilder webApplicationBuilder,
        string configurationSection = "Plugins",
        ILogger? bootstrapLogger = null
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(webApplicationBuilder, nameof(webApplicationBuilder));

        // Log what we are about to do.
        bootstrapLogger?.LogDebug(
            "Clearing any existing Blazor plugin resources."
            );

        // Clear any old blazor resources.
        BlazorResources.Clear();

        // Log what we are about to do.
        bootstrapLogger?.LogDebug(
            "Fetching the configuration section: {section}, " +
            "for the plugin loader",
            configurationSection
            );

        // Get the configuration section.
        var section = webApplicationBuilder.Configuration.GetSection(
            configurationSection
            );

        // Log what we are about to do.
        bootstrapLogger?.LogDebug(
            "Configuring the plugin options, for the plugin loader"
            );

        // Configure the plugin options.
        webApplicationBuilder.Services.ConfigureOptions<BlazorPluginsOptions>(
            section,
            out var pluginOptions
            );

        // Log what we are about to do.
        bootstrapLogger?.LogDebug(
            "Getting the list of modules for the plugin loader"
            );

        // Get the list of current enabled plugin modules.
        var modules = pluginOptions.Modules.Where(x => 
            x.IsEnabled
            );

        // Log what we are about to do.
        bootstrapLogger?.LogInformation(
            "Looping through {count} enabled plugin modules, " +
            "for the plugin loader",
            modules.Count()
            );

        // Loop through the modules.
        var index = -1;
        var asmNameSet = new HashSet<string>();
        foreach (var module in modules)
        {
            index++; // Used for extracting configuration sections.

            // Log what we are about to do.
            bootstrapLogger?.LogDebug(
                "Deciding whether the assembly name is a path, or " +
                "not, for the plugin loader."
                );

            Assembly? asm = null;

            // If the AssemblyNameOrPath ends with a .dll then we'll assume the
            //   property contains a path and treat it as such.
            if (module.AssemblyNameOrPath.EndsWith(".dll"))
            {
                // Log what we are about to do.
                bootstrapLogger?.LogDebug(
                    "Deciding whether the assembly path is rooted, or " +
                    "not, for the plugin loader."
                    );

                // Check for relative paths.
                if (false == Path.IsPathRooted(module.AssemblyNameOrPath))
                {
                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Building a complete path to a plugin assembly, " +
                        "for the plugin loader"
                        );

                    // Expand the path (the load expects a rooted path).
                    var completePath = Path.GetFullPath(
                        module.AssemblyNameOrPath
                        );

                    // Log what we are about to do.
                    bootstrapLogger?.LogInformation(
                        "Loading assembly by path: {path}, for the plugin " +
                        "loader",
                        completePath
                        );

                    // Load the assembly from the path.
                    asm = Assembly.LoadFile(
                        completePath
                        );
                }
                else
                {
                    try
                    {
                        // Log what we are about to do.
                        bootstrapLogger?.LogInformation(
                            "Loading assembly by name: {name}, for the " +
                            "plugin loader",
                            module.AssemblyNameOrPath
                            );

                        // Load the assembly from the path.
                        asm = Assembly.Load(
                            new AssemblyName(module.AssemblyNameOrPath)
                            );
                    }
                    catch (FileNotFoundException ex)
                    {
                        // Provide better context for the error.
                        throw new BlazorPluginException(
                            innerException: ex,
                            message: "When loading from an assembly name, remember that the " +
                            "assembly itself must have been loaded through a project reference. " +
                            "To dynamically load a plugin assembly, use a path to the assembly, " +
                            "instead of a name."
                            );
                    }
                }
            }
            else
            {
                try
                {
                    // Log what we are about to do.
                    bootstrapLogger?.LogInformation(
                        "Loading assembly by name: {name}, for the plugin " +
                        "loader",
                        module.AssemblyNameOrPath
                        );

                    // Load the assembly by name.
                    asm = Assembly.Load(
                        new AssemblyName(module.AssemblyNameOrPath)
                        );
                }
                catch (FileNotFoundException ex)
                {
                    // Provide better context for the error.
                    throw new BlazorPluginException(
                        innerException: ex,
                        message: "When loading from an assembly name, remember that the " +
                        "assembly itself must have been loaded through a project reference. " +
                        "To dynamically load a plugin assembly, use a path to the assembly, " +
                        "instead of a name."
                        );
                }
            }

            try
            {
                // Attempt to load all the dependencies for the assembly.
                Package.ParseDependencies(asm);
            }
            catch (Exception ex)
            {
                // Provide better context for the error.
                throw new BlazorPluginException(
                    innerException: ex,
                    message: $"While loading the assembly: '{asm.FullName}' one or more of " +
                    "its dependencies failed to load. This usually happens because a plugin " +
                    "was copied to a new location without also copying all the dependencies of " +
                    "that plugin. See inner exceptions for more detail."
                    );
            }

            // Log what we are about to do.
            bootstrapLogger?.LogDebug(
                "Fetching the assembly name from a plugin, for the " +
                "plugin loader."
                );

            // Create a safe name for the assembly.
            var safeAsmName = asm.GetName().Name;

            // Have we already processed this plugin assembly?
            if (asmNameSet.Contains(safeAsmName ?? ""))
            {
                continue; // Nothing to do.
            }

            // Does the module require Blazor routing support?
            if (module.Routed)
            {
                // Log what we are about to do.
                bootstrapLogger?.LogInformation(
                    "Marking the assembly for routing support, for the " +
                    "plugin loader"
                    );

                // Remember the assembly on behalf of Blazor.
                BlazorResources.RoutedAssemblies.Add(
                    asm
                    );
            }

            // Log what we are about to do.
            bootstrapLogger?.LogDebug(
                "Fetching the static resources (if any) from the assembly " +
                ", for the plugin loader"
                );

            // Get the static resources from the assembly.
            var staticResourceNames = asm.GetManifestResourceNames();

            // Log what we are about to do.
            bootstrapLogger?.LogDebug(
                "Building links to style sheets, for the plugin loader."
                );

            // Add links for any embedded style sheets.
            BuildStyleSheetLinks(
                asm,
                staticResourceNames,
                module,
                bootstrapLogger
                );

            // Log what we are about to do.
            bootstrapLogger?.LogDebug(
                "Building links to scripts, for the plugin loader."
                );

            // Add tags for any embedded scripts.
            BuildScriptTags(
                asm,
                staticResourceNames,
                module,
                bootstrapLogger
                );

            // Was an entry point specifically configured?
            if (false == string.IsNullOrEmpty(module.EntryPoint))
            {
                // Log what we are about to do.
                bootstrapLogger?.LogInformation(
                    "Resolved custom entry point: {name}, for the " +
                    "plugin loader",
                    module.EntryPoint
                    );

                // Log what we are about to do.
                bootstrapLogger?.LogDebug(
                    "Loading module type: {name}, for the plugin " +
                    "loader",
                    module.EntryPoint
                    );

                // Try to load the module's type.
                var type = asm.GetType(
                    module.EntryPoint
                    );

                // Did we fail?
                if (null == type)
                {
                    // Panic!!
                    throw new BlazorPluginException(
                        message: $"Failed to resolve the module type: '{module.EntryPoint}', " +
                            $"it may not exist in assembly '{safeAsmName}' after all, or it may " +
                            $"be private. See inner exceptions for more detail."
                        );
                }

                try
                {
                    // Log what we are about to do.
                    bootstrapLogger?.LogInformation(
                        "Creating module instance for plugin: {name}, for the " +
                        "plugin loader",
                        module.EntryPoint
                        );

                    // Try to create an instance of the module.
                    if (Activator.CreateInstance(
                        (Type)(type ?? Type.Missing) // Make the compiler happy re: possible null reference.
                        ) is IModule moduleObj)
                    {
                        // Log what we are about to do.
                        bootstrapLogger?.LogDebug(
                            "Filtering down to the plugin's configuration " +
                            "section, for the plugin loader."
                            );

                        // Filter down to the section for this module. This way, each module
                        //   can add whatever it needs, to this section, for configuration,
                        //   and the whole thing will just work.
                        var moduleSection = section.GetSection(
                            $"Modules:{index}"
                            );

                        // Log what we are about to do.
                        bootstrapLogger?.LogInformation(
                            "Configuring plugin: {name}, for the plugin loader",
                            module.EntryPoint
                            );

                        // Register any services in the module.
                        moduleObj.ConfigureServices(
                            webApplicationBuilder,
                            moduleSection,
                            bootstrapLogger
                            );

                        // Log what we are about to do.
                        bootstrapLogger?.LogDebug(
                            "Adding the assembly to the cache, for the plugin loader."
                            );

                        // Since we've gone to all the trouble to create this module, and we
                        //    know we'll need it again, as part of the whole startup operation,
                        //    let's go ahead and cache it now so we don't have to re-create it,
                        //    next time we need it.
                        BlazorResources.Modules.Add(moduleObj);
                    }
                }
                catch (Exception ex)
                {
                    // Provide more context for the error.
                    throw new BlazorPluginException(
                        message: $"Failed to create a '{module.EntryPoint}' module instance. " +
                            $"See inner exceptions for more detail.",
                        innerException: ex
                        );
                }
            }
            else
            {
                // Log what we are about to do.
                bootstrapLogger?.LogInformation(
                    "Resolving standard entry point: {name}, for the plugin loader",
                    $"{asm.GetName().Name}.Module"
                    );

                // If we get here then no entry point was explicitly configured, for
                //   this plugin. So, we'll follow our convention now and look for a
                //   public class in the root namespace (plugin's root), named 'Module'.
                // If we find such a module we'll try to use it here.

                // Try to load the module's type.
                var type = asm.GetType(
                    $"{asm.GetName().Name}.Module"
                    );

                // Did we find one?
                if (null != type)
                {
                    try
                    {
                        // Log what we are about to do.
                        bootstrapLogger?.LogInformation(
                            "Creating module instance for plugin: {name}, for the " +
                            "plugin loader",
                            module.EntryPoint
                            );

                        // Try to create an instance of the module.
                        if (Activator.CreateInstance(
                            (Type)(type ?? Type.Missing) // Make the compiler happy re: possible null reference.
                            ) is IModule moduleObj)
                        {
                            // Log what we are about to do.
                            bootstrapLogger?.LogDebug(
                                "Filtering down to the plugin's configuration section, " +
                                "for the plugin loader"
                                );

                            // Filter down to the section for this module. This way, each module
                            //   can add whatever it needs, to this section, for configuration,
                            //   and the whole thing will just work.
                            var moduleSection = section.GetSection(
                                $"Modules:{index}"
                                );

                            // Log what we are about to do.
                            bootstrapLogger?.LogInformation(
                                "Configuring plugin: {name}, for the plugin loader",
                                module.EntryPoint
                                );

                            // Register any services in the module.
                            moduleObj.ConfigureServices(
                                webApplicationBuilder,
                                moduleSection,
                                bootstrapLogger
                                );

                            // Log what we are about to do.
                            bootstrapLogger?.LogDebug(
                                "Adding the assembly to the cache, for the plugin loader."
                                );

                            // Since we've gone to all the trouble to create this module, and we
                            //    know we'll need it again, as part of the whole startup operation,
                            //    let's go ahead and cache it now, so we don't have to re-create it,
                            //    next time we need it.
                            BlazorResources.Modules.Add(moduleObj);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Provide more context for the error.
                        throw new BlazorPluginException(
                            message: $"Failed to create a '{module.EntryPoint}' module instance. " +
                                $"See inner exceptions for more detail.",
                            innerException: ex
                            );
                    }
                }
            }
        }

        // Return the service collection.
        return webApplicationBuilder;
    }

    #endregion

    // *******************************************************************
    // Private methods.
    // *******************************************************************

    #region Private methods

    /// <summary>
    /// This method builds tags for embedded scripts.
    /// </summary>
    /// <param name="asm">The assembly to use for the operation.</param>
    /// <param name="staticResourceNames">The static resources available 
    /// in the assembly.</param>
    /// <param name="module">The options for the module.</param>
    /// <param name="bootstrapLogger">An optional bootstrap logger to
    /// use for the operation.</param>
    private static void BuildScriptTags(
        Assembly asm,
        string[] staticResourceNames,
        BlazorModuleOptions module,
        ILogger? bootstrapLogger
        )
    {
        // Does the module contain scripts?
        if (null != module.Scripts)
        {
            // Log what we are about to do.
            bootstrapLogger?.LogDebug(
                "Looping through {count} scripts, for the plugin loader",
                module.Scripts.Count()
                );

            // Loop through all the scripts.
            foreach (var resource in module.Scripts)
            {
                // Log what we are about to do.
                bootstrapLogger?.LogDebug(
                    "Checking for embedded HTML in the link, for the " +
                    "plugin loader"
                    );

                // Check for embedded html in the path.
                if (resource.IsHTML())
                {
                    // Panic!
                    throw new InvalidOperationException(
                        message: $"It appears the script path '{resource}' " +
                            $"contains HTML. HTML is not allowed in the path."
                        );
                }

                // Format a script tag and save it.
                if (resource.StartsWith('/'))
                {
                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Validating the script link, for the plugin loader"
                        );

                    // Check for the resource in the assembly.
                    if (staticResourceNames.Contains(
                        $"{asm.GetName().Name}.wwwroot.{resource[1..]}")
                        )
                    {
                        // Panic!
                        throw new InvalidOperationException(
                            message: $"It appears the script '{resource}' " +
                                $"is not an embedded resource in assembly '{asm.GetName().Name}'!"
                            );
                    }

                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Adding a script link to the resource cache, for the " +
                        "plugin loader"
                        );

                    // Add the link.
                    BlazorResources.Scripts.Add(
                        $"<script src=\"_content/{asm.GetName().Name}{resource}\"></script>"
                        );
                }
                else
                {
                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Validating the script link, for the plugin loader"
                        );

                    // Check for the resource in the assembly.
                    if (false == staticResourceNames.Contains(
                        $"{asm.GetName().Name}.wwwroot.{resource}")
                        )
                    {
                        // Panic!
                        throw new BlazorPluginException(
                            message: $"It appears the script '{resource}' " +
                                $"is not an embedded resource in assembly '{asm.GetName().Name}'!"
                            );
                    }

                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Adding a script link to the resource cache, for " +
                        "the plugin loader"
                        );

                    // Add the link.
                    BlazorResources.Scripts.Add(
                        $"<script src=\"_content/{asm.GetName().Name}/{resource}\"></script>"
                        );
                }
            }
        }
    }

    // *******************************************************************

    /// <summary>
    /// This method builds links for embedded style sheets.
    /// </summary>
    /// <param name="asm">The assembly to use for the operation.</param>
    /// <param name="staticResourceNames">The static resources available 
    /// in the assembly.</param>
    /// <param name="module">The options for the module.</param>
    /// <param name="bootstrapLogger">An optional bootstrap logger to
    /// use for the operation.</param>
    private static void BuildStyleSheetLinks(
        Assembly asm,
        string[] staticResourceNames,
        BlazorModuleOptions module,
        ILogger? bootstrapLogger
        )
    {
        // Does the module contain stylesheets?
        if (null != module.StyleSheets)
        {
            // Log what we are about to do.
            bootstrapLogger?.LogDebug(
                "Looping through {count} style sheets, for the " +
                "plugin loader",
                module.StyleSheets.Count()
                );

            // Loop through all the style sheets.
            foreach (var resource in module.StyleSheets)
            {
                // Log what we are about to do.
                bootstrapLogger?.LogDebug(
                    "Checking for embedded HTML in the link, for " +
                    "the plugin loader"
                    );

                // Check for embedded html in the path.
                if (resource.IsHTML())
                {
                    // Panic!
                    throw new InvalidOperationException(
                        message: $"It appears the style sheet link '{resource}' " +
                            $"contains HTML. HTML is not allowed in the link."
                        );
                }

                // Format a link and save it.
                if (resource.StartsWith('/'))
                {
                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Validating the style sheet link, for the " +
                        "plugin loader"
                        );

                    // Check for the resource in the assembly.
                    if (staticResourceNames.Contains(
                        $"{asm.GetName().Name}.wwwroot.{resource[1..]}")
                        )
                    {
                        // Panic!
                        throw new BlazorPluginException(
                            message: $"It appears the style sheet link '{resource}' " +
                                $"is not an embedded resource in assembly '{asm.GetName().Name}'!"
                            );
                    }

                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Adding a stylesheet link to the resource cache, " +
                        "for the plugin loader"
                        );

                    // Add the link.
                    BlazorResources.StyleSheets.Add(
                        $"<link rel=\"stylesheet\" href=\"_content/{asm.GetName().Name}{resource}\" />"
                        );
                }
                else
                {
                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Validating the style sheet link, for the plugin loader"
                        );

                    // Check for the resource in the assembly.
                    if (false == staticResourceNames.Contains(
                        $"{asm.GetName().Name}.wwwroot.{resource}")
                        )
                    {
                        // Panic!
                        throw new BlazorPluginException(
                            message: $"It appears the style sheet link '{resource}' " +
                            $"is not an embedded resource in assembly '{asm.GetName().Name}'!"
                            );
                    }

                    // Log what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Adding a stylesheet link to the resource cache, " +
                        "for the plugin loader"
                        );

                    // Add the link.
                    BlazorResources.StyleSheets.Add(
                        $"<link rel=\"stylesheet\" href=\"_content/{asm.GetName().Name}/{resource}\" />"
                        );
                }
            }
        }
    }

    #endregion
}