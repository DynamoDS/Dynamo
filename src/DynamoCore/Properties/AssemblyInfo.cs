using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DynamoCore")]

[assembly: InternalsVisibleTo("DynamoSandbox")]
[assembly: InternalsVisibleTo("DynamoCoreWpf")]
[assembly: InternalsVisibleTo("DynamoCoreTests")]
[assembly: InternalsVisibleTo("DynamoCoreWpfTests")]
[assembly: InternalsVisibleTo("DynamoRevitDS")]
[assembly: InternalsVisibleTo("DynamoMSOfficeTests")]
[assembly: InternalsVisibleTo("CoreNodeModels")]
[assembly: InternalsVisibleTo("CoreNodeModelsWpf")]
[assembly: InternalsVisibleTo("Watch3DNodeModelsWpf")]
[assembly: InternalsVisibleTo("DynamoApplications")]
[assembly: InternalsVisibleTo("GeometryUIWpf")]
[assembly: InternalsVisibleTo("GeometryUI")]
[assembly: InternalsVisibleTo("RevitSystemTests")]
[assembly: InternalsVisibleTo("DynamoCLI")]
[assembly: InternalsVisibleTo("DynamoWPFCLI")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // Dynamic assembly created by Moq
[assembly: InternalsVisibleTo("WpfVisualizationTests")]
[assembly: InternalsVisibleTo("PackageDetailsViewExtension")]
[assembly: InternalsVisibleTo("ViewExtensionLibraryTests")]
[assembly: InternalsVisibleTo("DynamoPerformanceTests")]
[assembly: InternalsVisibleTo("PackageManagerTests")]
// Internals are visible to the Package Manager extension
// For workspace package dependency collection
[assembly: InternalsVisibleTo("DynamoPackages")]
[assembly: InternalsVisibleTo("WorkspaceDependencyViewExtension")]
[assembly: InternalsVisibleTo("DynamoMLDataPipeline")]
[assembly: InternalsVisibleTo("PythonNodeModelsWpf")]
[assembly: InternalsVisibleTo("PythonNodeModels")]
[assembly: InternalsVisibleTo("LibraryViewExtensionWebView2")]
[assembly: InternalsVisibleTo("PythonMigrationViewExtension")]
[assembly: InternalsVisibleTo("NodeDocumentationMarkdownGenerator")]
[assembly: InternalsVisibleTo("LintingViewExtension")]
[assembly: InternalsVisibleTo("GenerativeDesign.Dynamo.ViewExtension")]
[assembly: InternalsVisibleTo("GenerativeDesign.Dynamo.PackAndGo")]
[assembly: InternalsVisibleTo("DynamoPlayer.Extension")]
[assembly: InternalsVisibleTo("DynamoPlayer.Workflows")]
[assembly: InternalsVisibleTo("DynamoPlayer")]
[assembly: InternalsVisibleTo("DynamoConnector")]
[assembly: InternalsVisibleTo("DSCPython")]
[assembly: InternalsVisibleTo("DynamoPythonTests")]
[assembly: InternalsVisibleTo("GraphMetadataViewExtension")]
[assembly: InternalsVisibleTo("SystemTestServices")]
[assembly: InternalsVisibleTo("DynamoManipulation")]
[assembly: InternalsVisibleTo("IronPythonTests")]
[assembly: InternalsVisibleTo("DynamoPackagesWPF")]
[assembly: InternalsVisibleTo("GraphNodeManagerViewExtension")]
[assembly: InternalsVisibleTo("ExportSampleImagesViewExtension")]
[assembly: InternalsVisibleTo("DocumentationBrowserViewExtension")]
[assembly: InternalsVisibleTo("Notifications")]

// Disable PublicAPIAnalyzer errors for this type as they're already added to the public API text file
#pragma warning disable RS0016 
[assembly: TypeForwardedTo(typeof(Dynamo.Scheduler.Disposable))]
#pragma warning restore RS0016
