
using Microsoft.AspNetCore.Builder;

namespace CG.Blazor.Plugins;

/// <summary>
/// This class utility contains logic to parse .NET packages.
/// </summary>
internal class Package
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method walks through the tree of dependencies for the given 
    /// assembly, and loads each child.
    /// </summary>
    /// <param name="assembly">The assembly to use for the operation.</param>
    /// <exception cref="FileNotFoundException">This exception is thrown
    /// whenever a dependency of the given assembly couldn't be found.</exception>
    /// <exception cref="FileLoadException">This exception is thrown whenever
    /// the assembly loader failed to load one or more assemblies.</exception>
    /// <exception cref="BadImageFormatException">This exception is thrown 
    /// whenever one or more assemblies had a bad format.</exception>
    /// <exception cref="ArgumentException">This exception is thrown whenever
    /// one or more arguments were missing, or invalid.</exception>
    public static void ParseDependencies(
        Assembly assembly
        )
    {
        // Validate the parameters before attempting to use them.
        Guard.Instance().ThrowIfNull(assembly, nameof(assembly));

        // To track which assemblies have been already processed. For 
        //   our purposes, we'll assume anything that's already in the 
        //   app-domain has been fully loaded.
        var processedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .ToDictionary(x => x.FullName ?? "", x => x);

        // Get the dependencies for the assembly.
        var refAssemblyNames = assembly.GetReferencedAssemblies();

        // Loop through the dependencies.
        Assembly? refAssembly = null;
        foreach (var refAssemblyName in refAssemblyNames)
        {
            // Have we already processed this assembly?
            if (processedAssemblies.ContainsKey(
                refAssemblyName.FullName
                ))
            {
                continue; // Nothing to do.
            }

            try
            {
                // Load the assembly.
                refAssembly = Assembly.Load(
                    refAssemblyName
                    );
            }
            catch (FileNotFoundException ex)
            {
                // If we get here then a dependent assembly couldn't be found 
                //   in any of the "normal" .NET paths. So, let's see if we
                //   can find the dependent assembly in whatever folder it's
                //   parent is currently in. Pfft, worth a shot, anyway.

                // Get the location of the parent assembly.
                var parentPath = Path.GetDirectoryName(
                    assembly.Location
                    ) ?? "";

                // Build a potential path to the assembly.
                var assemblyPath = Path.Combine(
                    parentPath,
                    $"{refAssemblyName.Name}.dll"
                    );

                // Does the assembly file exist?
                if (File.Exists(assemblyPath))
                {
                    // If we get here then we've found the dependent assembly
                    // so now we'll try to load it and continue processing 
                    // other dependencies.

                    try
                    {
                        // Load the assembly by path.
                        refAssembly = Assembly.LoadFrom(
                            assemblyPath
                            );
                    }
                    catch (FileNotFoundException)
                    {
                        // If we get here then we still failed to load the 
                        //   dependent assembly, even using the new path, so,
                        //   pffft, time to give up.

                        // Rethrow the original exception.
                        throw ex;
                    }
                }
                else
                {
                    // If we get here then the dependent assembly wasn't in the 
                    //   folder with it's parent. Pfft, time to give up.

                    // Rethrow the original exception.
                    throw ex;
                }
            }

            // Did we load the assembly?
            if (refAssembly is not null)
            {
                // Remember that we loaded this assembly.
                processedAssemblies[refAssemblyName.FullName] = refAssembly;

                // Parse down the tree.
                ParseDependencies(
                    refAssembly,
                    assembly,
                    processedAssemblies
                    );
            }
        }
    }

    #endregion

    // *******************************************************************
    // Private methods.
    // *******************************************************************

    #region Private methods

    /// <summary>
    /// This method walks through the tree of dependencies for the given 
    /// assembly, and loads each child.
    /// </summary>
    /// <param name="assembly">The assembly to use for the operation.</param>
    /// <param name="parentAssembly">The parent reference for this assembly.</param>
    /// <param name="processedAssemblies">The table of processed assemblies.</param>
    public static void ParseDependencies(
        Assembly assembly,
        Assembly parentAssembly,
        Dictionary<string, Assembly> processedAssemblies
        )
    {
        // Get the referenced assembly names
        var refAssemblyNames = assembly.GetReferencedAssemblies();

        // Loop through the referenced assemblies.
        Assembly? refAssembly = null;
        foreach (var refAssemblyName in refAssemblyNames)
        {
            // Have we already processed this assembly?
            if (processedAssemblies.ContainsKey(refAssemblyName.FullName))
            {
                continue; // Nothing to do.
            }

            try
            {
                // Load the assembly.
                refAssembly = Assembly.Load(
                    refAssemblyName
                    );
            }
            catch (FileNotFoundException ex)
            {
                // If we get here then a dependent assembly couldn't be found 
                //   in any of the "normal" .NET paths. So, let's see if we
                //   can find the dependent assembly in whatever folder it's
                //   parent is currently in. Pfft, worth a shot, anyway.

                // Get the location of the parent assembly.
                var parentPath = Path.GetDirectoryName(
                    assembly.Location
                    ) ?? "";

                // Build a potential path to the assembly.
                var assemblyPath = Path.Combine(
                    parentPath,
                    $"{refAssemblyName.Name}.dll"
                    );

                // Does the assembly file exist?
                if (File.Exists(assemblyPath))
                {
                    // If we get here then we've found the dependent assembly
                    // so now we'll try to load it and continue processing 
                    // other dependencies.

                    try
                    {
                        // Load the assembly by path.
                        refAssembly = Assembly.LoadFrom(
                            assemblyPath
                            );
                    }
                    catch (Exception)
                    {
                        // If we get here then we still failed to load the 
                        //   dependent assembly, so, pffft, time to give up.

                        // Rethrow the original exception.
                        throw ex;
                    }
                }
                else
                {
                    // If we get here then the dependent assembly wasn't in the 
                    //   folder with it's parent. Pfft, time to give up.

                    // Rethrow the original exception.
                    throw ex;
                }
            }

            // Did we load the assembly?
            if (refAssembly is not null)
            {
                // Remember that we loaded this assembly.
                processedAssemblies[refAssemblyName.FullName] = refAssembly;

                // Parse down the tree.
                ParseDependencies(
                    refAssembly,
                    parentAssembly,
                    processedAssemblies
                    );
            }
        }
    }

    #endregion
}
