using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DynamoCore")]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]

[assembly: InternalsVisibleTo("DynamoCoreWpf")]
[assembly: InternalsVisibleTo("DynamoCoreTests")]
[assembly: InternalsVisibleTo("DynamoCoreUITests")]
[assembly: InternalsVisibleTo("DynamoRevitDS")]
[assembly: InternalsVisibleTo("DynamoMSOfficeTests")]
[assembly: InternalsVisibleTo("DSCoreNodesUI")]
[assembly: InternalsVisibleTo("DynamoWebServer")]
