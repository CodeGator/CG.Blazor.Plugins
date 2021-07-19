using CG;
using CG.Blazor.Plugins;
using CG.Blazor.Plugins.Options;
using CG.DataProtection;
using CG.Runtime;
using CG.Validations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// This class contains extension methods related to the <see cref="IServiceCollection"/>
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method adds any configured plugins to the specified service
        /// collection. It also registers any resources from the plugins, with 
        /// Blazor. It also makes the Blazor router aware of any components or
        /// pages that require runtime routing support.
        /// </summary>
        /// <param name="serviceCollection">The service collection to use for 
        /// the operation.</param>
        /// <param name="configuration">The configuration to use for the operation.</param>
        /// <returns>The value of the <paramref name="serviceCollection"/>
        /// parameter, for chaining calls together.</returns>
        /// <exception cref="ArgumentException">This exception is thrown whenever
        /// one or more arguments is missing, or invalid.</exception>
        /// <exception cref="BlazorPluginException">This exception is thrown whenever
        /// the operation fails.</exception>
        /// <remarks>
        /// <para>
        /// In the interest of flexibility, this method no longer attempts to
        /// locate a specific section in the configuration. That way, the section
        /// for plugins can be named anything and be located anywhere. But, 
        /// everything comes at a cost and for this, the cost is, the developer
        /// has to ensure that the <paramref name="configuration"/> object is 
        /// pointed to whatever section you're using, for Blazor plugins. If you
        /// don't do that, it won't work, plain and simple.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddBlazorPlugins(
            this IServiceCollection serviceCollection,
            IConfiguration configuration
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(serviceCollection, nameof(serviceCollection))
                .ThrowIfNull(configuration, nameof(configuration));

            // Call the overload.
            return serviceCollection.AddBlazorPlugins(
                DataProtector.Instance(), // Default data protector.
                configuration
                );
        }

        // *******************************************************************

        /// <summary>
        /// This method adds any configured plugins to the specified service
        /// collection. It also registers any resources from the plugins, with 
        /// Blazor. It also makes the Blazor router aware of any components or
        /// pages that require runtime routing support.
        /// </summary>
        /// <param name="serviceCollection">The service collection to use for 
        /// the operation.</param>
        /// <param name="dataProtector">The data protector to use for the operation.</param>
        /// <param name="configuration">The configuration to use for the operation.</param>
        /// <returns>The value of the <paramref name="serviceCollection"/>
        /// parameter, for chaining calls together.</returns>
        /// <exception cref="ArgumentException">This exception is thrown whenever
        /// one or more arguments is missing, or invalid.</exception>
        /// <exception cref="BlazorPluginException">This exception is thrown whenever
        /// the operation fails.</exception>
        /// <remarks>
        /// <para>
        /// In the interest of flexibility, this method no longer attempts to
        /// locate a specific section in the configuration. That way, the section
        /// for plugins can be named anything and be located anywhere. But, 
        /// everything comes at a cost and for this, the cost is, the developer
        /// has to ensure that the <paramref name="configuration"/> object is 
        /// pointed to whatever section you're using, for Blazor plugins. If you
        /// don't do that, it won't work, plain and simple.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddBlazorPlugins(
            this IServiceCollection serviceCollection,
            IDataProtector dataProtector,
            IConfiguration configuration
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(serviceCollection, nameof(serviceCollection))
                .ThrowIfNull(dataProtector, nameof(dataProtector))
                .ThrowIfNull(configuration, nameof(configuration));

            // Clear any old blazor resources.
            BlazorResources.Clear();

            // Configure the plugin options.
            serviceCollection.ConfigureOptions<BlazorPluginsOptions>(
                dataProtector,
                configuration,
                out var pluginOptions
                );

            var asmNameSet = new HashSet<string>();
            var loader = new AssemblyLoader();

            // Loop through the modules.
            var index = -1;
            foreach (var module in pluginOptions.Modules)
            {
                index++; // Used for extracting configuration sections.

                Assembly? asm = null;

                // If the AssemblyNameOrPath ends with a .dll then we'll assume the
                //   property contains a path and treat it as such.
                if (module.AssemblyNameOrPath.EndsWith(".dll"))
                {
                    // Check for relative paths.
                    if (false == Path.IsPathRooted(module.AssemblyNameOrPath))
                    {
                        // Expand the path (the load expects a rooted path).
                        var completePath = Path.GetFullPath(
                            module.AssemblyNameOrPath
                            );

                        // Load the assembly from the path.
                        asm = loader.LoadFromAssemblyPath(
                            completePath
                            );
                    }
                    else
                    {
                        // Load the assembly from the path.
                        asm = loader.LoadFromAssemblyPath(
                            module.AssemblyNameOrPath
                            );
                    }
                }
                else
                {
                    // Create the complete assembly name.
                    var assemblyName = new AssemblyName(
                        module.AssemblyNameOrPath
                        );

                    // Load the assembly by name.
                    asm = loader.LoadFromAssemblyName(
                        assemblyName
                        );
                }

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
                    // Remember the assembly on behalf of Blazor.
                    BlazorResources.RoutedAssemblies.Add(
                        asm
                        );
                }

                // Get the static resources from the assembly.
                var staticResourceNames = asm.GetManifestResourceNames();

                // Add links for any embedded style sheets.
                BuildStyleSheetLinks(
                    asm,
                    staticResourceNames,
                    module
                    );

                // Add tags for any embedded scripts.
                BuildScriptTags(
                    asm,
                    staticResourceNames,
                    module
                    );

                // Is there a module?
                if (false == string.IsNullOrEmpty(module.EntryPoint))
                {
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
                        // Try to create an instance of the module.
                        if (Activator.CreateInstance(
                            (Type)(type ?? Type.Missing) // Make the compiler happy re: possible null reference.
                            ) is IModule moduleObj)
                        {
                            // Filter down to the section for this module. This way, each module
                            //   can add whatever it needs, to this section, for configuration,
                            //   and the whole thing will just work.
                            var moduleSection = configuration.GetSection(
                                $"Modules:{index}"
                                );

                            // Register any services in the module.
                            moduleObj.ConfigureServices(
                                serviceCollection,
                                moduleSection
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

            // Return the service collection.
            return serviceCollection;
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
        /// <param name="staticResourceNames">The static resources avaiable in the assmbly.</param>
        /// <param name="module">The options for the module.</param>
        private static void BuildScriptTags(
            Assembly asm,
            string[] staticResourceNames,
            BlazorModuleOptions module
            )
        {
            // Does the module contain scripts?
            if (null != module.Scripts)
            {
                // Loop through all the scripts.
                foreach (var resource in module.Scripts)
                {
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

                        // Add the link.
                        BlazorResources.Scripts.Add(
                            $"<script src=\"_content/{asm.GetName().Name}{resource}\"></script>"
                            );
                    }
                    else
                    {
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
        /// <param name="staticResourceNames">The static resources avaiable in the assmbly.</param>
        /// <param name="module">The options for the module.</param>
        private static void BuildStyleSheetLinks(
            Assembly asm,
            string[] staticResourceNames,
            BlazorModuleOptions module
            )
        {
            // Does the module contain stylesheets?
            if (null != module.StyleSheets)
            {
                // Loop through all the style sheets.
                foreach (var resource in module.StyleSheets)
                {
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

                        // Add the link.
                        BlazorResources.StyleSheets.Add(
                            $"<link rel=\"stylesheet\" href=\"_content/{asm.GetName().Name}{resource}\" />"
                            );
                    }
                    else
                    {
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
}
