using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
using System.Windows;

[assembly: AssemblyTitle("DynamoCoreWpf")]

[assembly: AssemblyCulture("")]



// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("92beb3ad-6772-4d4b-9a00-49e727fc12f5")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.ExternalAssembly, //where theme specific resource dictionaries are located
                                                 //(used if a resource is not found in the page, 
                                                 // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page, 
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: InternalsVisibleTo("DynamoCoreTests")]
[assembly: InternalsVisibleTo("DynamoCoreWpfTests")]
[assembly: InternalsVisibleTo("DynamoMSOfficeTests")]
[assembly: InternalsVisibleTo("CoreNodeModelsWpf")]
[assembly: InternalsVisibleTo("DynamoSandbox")]
[assembly: InternalsVisibleTo("WpfVisualizationTests")]
[assembly: InternalsVisibleTo("DynamoPackagesUITests")]
[assembly: InternalsVisibleTo("ViewExtensionLibraryTests")]
[assembly: InternalsVisibleTo("DynamoWPFCLI")]
[assembly: InternalsVisibleTo("CommandLineTests")]
[assembly: InternalsVisibleTo("Watch3DNodeModelsWpf")]
[assembly: InternalsVisibleTo("DynamoPerformanceTests")]
[assembly: InternalsVisibleTo("WorkspaceDependencyViewExtension")]
[assembly: InternalsVisibleTo("DocumentationBrowserViewExtension")]
[assembly: InternalsVisibleTo("SystemTestServices")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("PythonNodeModelsWpf")]
[assembly: InternalsVisibleTo("LintingViewExtension")]
[assembly: InternalsVisibleTo("GraphMetadataViewExtension")]
[assembly: InternalsVisibleTo("PackageDetailsViewExtension")]
[assembly: InternalsVisibleTo("Notifications")]
[assembly: InternalsVisibleTo("LibraryViewExtensionWebView2")]
[assembly: InternalsVisibleTo("PythonNodeModelsWpf")]
[assembly: InternalsVisibleTo("IronPythonTests")]
[assembly: InternalsVisibleTo("DynamoPackagesWPF")]
[assembly: InternalsVisibleTo("DynamoPlayerExtension")]


[assembly: TypeForwardedTo(typeof(Dynamo.Wpf.Interfaces.LayoutSpecification))]
[assembly: TypeForwardedTo(typeof(Dynamo.Wpf.Interfaces.LayoutSection))]
[assembly: TypeForwardedTo(typeof(Dynamo.Wpf.Interfaces.LayoutElement))]
[assembly: TypeForwardedTo(typeof(Dynamo.Wpf.Interfaces.LayoutElementType))]
[assembly: TypeForwardedTo(typeof(Dynamo.Wpf.Interfaces.LayoutIncludeInfo))]
[assembly: TypeForwardedTo(typeof(Dynamo.Wpf.Interfaces.ILibraryViewCustomization))]
