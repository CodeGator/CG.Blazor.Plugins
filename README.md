# CG.Blazor.Plugins: 

---
[![Build Status](https://dev.azure.com/codegator/CG.Blazor.Plugins/_apis/build/status/CodeGator.CG.Blazor.Plugins?branchName=main)](https://dev.azure.com/codegator/CG.Blazor.Plugins/_build/latest?definitionId=67&branchName=main)
[![Github docs](https://img.shields.io/static/v1?label=Documentation&message=online&color=blue)](https://codegator.github.io/CG.Blazor.Plugins/index.html)
[![NuGet downloads](https://img.shields.io/nuget/dt/CG.Blazor.Plugins.svg?style=flat)](https://nuget.org/packages/CG.Blazor.Plugins)
![Azure DevOps coverage](https://img.shields.io/azure-devops/coverage/codegator/CG.Blazor.Plugins/67)
[![Github discussion](https://img.shields.io/badge/Discussion-online-blue)](https://github.com/CodeGator/CG.Blazor.Plugins/discussions)
[![CG.Blazor.Plugins on fuget.org](https://www.fuget.org/packages/CG.Blazor.Plugins/badge.svg)](https://www.fuget.org/packages/CG.Blazor.Plugins)

#### What does it do?
The package contains server side Blazor plugin extensions used by other CodeGator packages.

#### Commonly used types:
* Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions
* CG.Blazor.Plugins.Plugins.IModule
* CG.Blazor.Plugins.Plugins.ModuleBase
* CG.Blazor.Plugins.Options.BlazorPluginOptions
* CG.Blazor.Plugins.Options.BlazorModuleOptions

#### What platform(s) does it support?
* .NET 5.x or higher

#### How do I install it?
The binary is hosted on [NuGet](https://www.nuget.org/packages/CG.Blazor.Plugins). To install the package using the NuGet package manager:

PM> Install-Package CG.Blazor.Plugins

#### How do I contact you?
If you've spotted a bug in the code please use the project Issues [HERE](https://github.com/CodeGator/CG.Blazor.Plugins/issues)

We have a discussion group [HERE](https://github.com/CodeGator/CG.Blazor.Plugins/discussions)

#### Is there any documentation?
There is developer documentation [HERE](https://codegator.github.io/CG.Blazor.Plugins/)

We also blog about projects like this one on our website, [HERE](http://www.codegator.com)

---

#### How do I get started?

There is a working quick start sample [HERE](https://github.com/CodeGator/CG.Blazor.Plugins/tree/main/samples)

Steps to add plugins to a Blazor project:

1. Create a Blazor project to suit your taste.

2. Add the CG.Blazor.Plugins NUGET package to the project.

3. Add the `@using CG.Blazor.Plugins` statement to the _Imports.razor file.

4. Add the line: `services.AddBlazorPlugins(Configuration.GetSection("Plugins"));` to the `ConfigureServices` method of the `Startup` class.

5. Add the line: `app.UseBlazorPlugins(env);` to the `Configure` method of the `Startup` class.

6. Add the following to your appsettings.json file:

```
{
  "Plugins": {
    "Modules": [
      {
        "AssemblyNameOrPath": "YourPlugin",
        "Routed": true // <- true if you have pages that need routing support.
      }
    ]
  }
}
```

Where you will, of course, use the name of your plugin assembly in place of `YourPlugin`

7. Edit the App.razor file like so:

```
<Router AppAssembly="@typeof(Program).Assembly" 
        PreferExactMatches="@true"
        AdditionalAssemblies=@BlazorResources.RoutedAssemblies> // <-- this line
    // code removed for clarity.
</Router>
```

The indicated line should be added, so the framework will route properly for the plugins.

8. Edit the _Host.cshtml file like so:

8A. Add the `@using CG.Blazor.Plugins` statement near the top of the file.

8B. Add the `@(Html.Raw(BlazorResources.RenderStyleSheetLinks()))` statement at the bottom of the `head` tag

8C. Add the `@(Html.Raw(BlazorResources.RenderScriptTags()))` statement at the bottom of the `body` tag.

9. Create your plugin project and add a reference to that project to your Blazor project.

That's it! Of course, you could get fancier with things, if you like. But, this is enough to get started.










