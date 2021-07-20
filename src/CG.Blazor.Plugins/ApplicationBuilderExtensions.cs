using CG.Blazor.Plugins;
using CG.Blazor.Plugins.Options;
using CG.Runtime;
using CG.Validations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// This class contains extension methods related to the <see cref="IApplicationBuilder"/>
    /// </summary>
    public static partial class ApplicationBuilderExtensions
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method wires up services needed to support Blazor plugins.
        /// </summary>
        /// <param name="applicationBuilder">The application builder to use 
        /// for the operation.</param>
        /// <param name="webHostEnvironment">The hosting environment to use 
        /// for the operation.</param>
        /// <returns>the value of the <paramref name="applicationBuilder"/>
        /// parameter, for chaining method calls together.</returns>
        /// <exception cref="ArgumentException">This exception is thrown whenever
        /// one or more arguments is missing, or invalid.</exception>
        /// <exception cref="BlazorPluginException">This exception is thrown whenever
        /// the operation fails.</exception>
        public static IApplicationBuilder UseBlazorPlugins(
            this IApplicationBuilder applicationBuilder,
            IWebHostEnvironment webHostEnvironment
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(applicationBuilder, nameof(applicationBuilder))
                .ThrowIfNull(webHostEnvironment, nameof(webHostEnvironment));

            // One of the things we need to do, in order to support static resources
            //   in late-bound, dynamically loaded assemblies, is to create an embedded
            //   file provider for each plugin assembly and wire everything together with
            //   a composite file provider. We'll do that here.

            // Ensure we're setup to use static files.
            applicationBuilder.UseStaticFiles();

            var allProviders = new List<IFileProvider>();

            // Is there already a file provider?
            if (webHostEnvironment.WebRootFileProvider != null)
            {
                // Add the existing file provider.
                allProviders.Add(
                    webHostEnvironment.WebRootFileProvider
                    );
            }

            var asmNameSet = new HashSet<string>();

            // Add providers for any embedded style sheets.
            BuildStyleSheetProviders(
                asmNameSet,
                allProviders
                );

            // Add providers for any embedded scripts.
            BuildScriptProviders(
                asmNameSet,
                allProviders
                );

            // Get the plugin options.
            var options = applicationBuilder.ApplicationServices
                .GetRequiredService<IOptions<BlazorPluginsOptions>>();

            // Add any remaining providers.
            BuildRemainingProviders(
                options,
                asmNameSet,
                allProviders
                );

            // Replace the existing file provider with a composite provider.
            webHostEnvironment.WebRootFileProvider = new CompositeFileProvider(
                allProviders
                );

            var errors = new List<Exception>();

            // The final thing we need to do is walk through the list of modules
            //   and call the Configure method on each one, just in case any of 
            //   them are expecting that to happen.
            foreach (var module in BlazorResources.Modules)
            {
                try
                {
                    // Configure any services in the module.
                    module.Configure(
                        applicationBuilder,
                        webHostEnvironment
                        );
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            // Were there errors?
            if (errors.Any())
            {
                throw new AggregateException(
                    message: $"One or more errors were detected while configuring " +
                        $"Blazor plugin modules. See inner exceptions for more details.",
                    innerExceptions: errors
                    );
            }

            // At this point we clear the cached modules because we no longer
            //   require those resources in memory.
            BlazorResources.Modules.Clear();

            // Return the application builder.
            return applicationBuilder;
        }

        #endregion

        // *******************************************************************
        // Private methods.
        // *******************************************************************

        #region Private methods

        /// <summary>
        /// This method looks through the script tags in the <see cref="BlazorResources.Scripts"/>
        /// collection and ensures that each plugin has a file provider in the
        /// <paramref name="allProviders"/> collection, to read the static 
        /// resource at runtime.
        /// </summary>
        /// <param name="asmNameSet">The set of all previously processed plugin
        /// assemblies.</param>
        /// <param name="allProviders">The list of all previously added file
        /// providers.</param>
        private static void BuildScriptProviders(
            HashSet<string> asmNameSet,
            List<IFileProvider> allProviders
            )
        {
            // Loop through all the script tags.
            foreach (var resource in BlazorResources.Scripts)
            {
                // We won't check these tags for embedded HTML since that 
                //   was already done in the AddPlugins method.

                // Look for the leading content portion.
                var index1 = resource.IndexOf("_content/");
                if (index1 == -1)
                {
                    // Panic.
                    throw new BlazorPluginException(
                        message: $"It appears the script tag '{resource}' " +
                            $"is missing the '_content/' portion of the tag."
                        );
                }

                // Adjust the index.
                index1 += "_content/".Length;

                // Look for the first '/' character.
                var index2 = resource.IndexOf("/", index1);
                if (index2 == -1)
                {
                    // Panic.
                    throw new BlazorPluginException(
                        message: $"It appears the script tag '{resource}'" +
                            $"is missing a '/' after the assembly name."
                        );
                }

                // Parse out the assembly name.
                var asmName = resource[index1..index2];

                // Have we already created a file provider for this assembly?
                if (asmNameSet.Contains(asmName))
                {
                    continue; // Nothing to do.
                }

                // If we get here then we need to create an embedded file provider
                //   for the plugin assembly.

                // Create a loader instance.
                var loader = new AssemblyLoader();

                Assembly? asm = null;
                try
                {
                    // Get the assembly reference.
                    asm = loader.LoadFromAssemblyName(
                        new AssemblyName(asmName)
                        );
                }
                catch (FileNotFoundException ex)
                {
                    // Provide better context for the error.
                    throw new BlazorPluginException(
                        message: $"It appears the plugin assembly '{asmName}' " +
                            $"can't be found. See inner exception for more detail.",
                        innerException: ex
                        );
                }

                try
                {
                    // Create a file provider to read embedded resources.
                    var fileProvider = new ManifestEmbeddedFileProviderEx(
                            asm,
                            $"wwwroot"
                            );

                    // Add the provider to the collection.
                    allProviders.Insert(0, fileProvider);
                }
                catch (InvalidOperationException)
                {
                    // Not really an error, the plugin assembly might not have,
                    // or event need an embedded stream of resources.
                }
            }
        }

        // *******************************************************************

        /// <summary>
        /// This method looks through the style sheet links in the <see cref="BlazorResources.StyleSheets"/>
        /// collection and ensures that each plugin has a file provider in the
        /// <paramref name="allProviders"/> collection, to read the static 
        /// resource at runtime.
        /// </summary>
        /// <param name="asmNameSet">The set of all previously processed plugin
        /// assemblies.</param>
        /// <param name="allProviders">The list of all previously added file
        /// providers.</param>
        private static void BuildStyleSheetProviders(
            HashSet<string> asmNameSet,
            List<IFileProvider> allProviders
            )
        {
            // Loop through all the style sheets links.
            foreach (var resource in BlazorResources.StyleSheets)
            {
                // We won't check these tags for embedded HTML since that 
                //   was already done in the AddPlugins method.

                // Look for the leading content portion.
                var index1 = resource.IndexOf("_content/");
                if (index1 == -1)
                {
                    // Panic.
                    throw new BlazorPluginException(
                        message: $"It appears the script tag '{resource}' " + 
                            $"is missing the '_content/' portion of the tag."
                        );
                }

                // Adjust the index.
                index1 += "_content/".Length;

                // Look for the first '/' character.
                var index2 = resource.IndexOf("/", index1);
                if (index2 == -1)
                {
                    // Panic.
                    throw new BlazorPluginException(
                        message: $"It appears the script tag '{resource}' " +
                            $"is missing a '/' after the assembly name."
                        );
                }

                // Parse out the assembly name.
                var asmName = resource[index1..index2];

                // Have we already created a file provider for this assembly?
                if (asmNameSet.Contains(asmName))
                {
                    continue; // Nothing to do.
                }

                // If we get here then we need to create an embedded file provider
                //   for the plugin assembly.

                // Create a loader instance.
                var loader = new AssemblyLoader();

                Assembly? asm = null;
                try
                {
                    // Get the assembly reference.
                    asm = loader.LoadFromAssemblyName(
                        new AssemblyName(asmName)
                        );
                }
                catch (FileNotFoundException ex)
                {
                    // Provide better context for the error.
                    throw new BlazorPluginException(
                        message: $"It appears the plugin assembly '{asmName}' " +
                            $"can't be found. See inner exception for more detail.",
                        innerException: ex
                        );
                }

                try
                {
                    // Create a file provider to read embedded resources.
                    var fileProvider = new ManifestEmbeddedFileProviderEx(
                            asm,
                            $"wwwroot"
                            );

                    // Add the provider to the collection.
                    allProviders.Insert(0, fileProvider);
                }
                catch (InvalidOperationException)
                {
                    // Not really an error, the plugin assembly might not have,
                    // or event need an embedded stream of resources.
                }
            }
        }

        // *******************************************************************

        /// <summary>
        /// This method adds a file provider for any plugin assembly that doesn't
        /// contain links to embedded style sheets, or javascripts. 
        /// </summary>
        /// <param name="options">The options to use for the operation.</param>
        /// <param name="asmNameSet">The set of all previously processed plugin
        /// assemblies.</param>
        /// <param name="allProviders">The list of all previously added file
        /// providers.</param>
        private static void BuildRemainingProviders(
            IOptions<BlazorPluginsOptions> options,
            HashSet<string> asmNameSet,
            List<IFileProvider> allProviders
            )
        {
            // Create a loader instance.
            var loader = new AssemblyLoader();

            // Loop through all the plugin modules.
            foreach (var module in options.Value.Modules)
            {
                Assembly? asm;

                // Is this module configured with a path?
                if (module.AssemblyNameOrPath.EndsWith(".dll"))
                {
                    // Strip out just the assembly file name.
                    var fileName = Path.GetFileNameWithoutExtension(
                        module.AssemblyNameOrPath
                        );

                    // Have we already processed this assembly?
                    if (asmNameSet.Contains(fileName))
                    {
                        continue; // Nothing left to do.
                    }

                    // Check for relative paths.
                    if (false == Path.IsPathRooted(
                        module.AssemblyNameOrPath
                        ))
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
                    // Have we already processed this assembly?
                    if (asmNameSet.Contains(module.AssemblyNameOrPath))
                    {
                        continue; // Nothing left to do.
                    }

                    // Make an assembly name.
                    var asmName = new AssemblyName(
                        module.AssemblyNameOrPath
                        );

                    // Load the assembly by name.
                    asm = loader.LoadFromAssemblyName(
                        asmName
                        );
                }

                // At this point we have a reference to the loaded assembly so we
                //   can use that to try to find an embedded resource manifest.

                try
                {
                    // Create a file provider to read embedded resources.
                    var fileProvider = new ManifestEmbeddedFileProviderEx(
                            asm,
                            $"wwwroot"
                            );

                    // Add the provider to the collection.
                    allProviders.Insert(0, fileProvider);
                }
                catch (InvalidOperationException)
                {
                    // Not really an error, the plugin assembly might not have,
                    // or event need an embedded stream of resources.
                }
            }
        }

        #endregion
    }
}
