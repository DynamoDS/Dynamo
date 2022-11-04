using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("PythonNodeModels")]
[assembly: AssemblyCulture("")]
// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a0debf47-0cf2-46e8-90b9-0c3d9b808fd7")]

// Remove this when PythonEngineManager become public
[assembly: InternalsVisibleTo("DynamoPythonTests")]
[assembly: InternalsVisibleTo("IronPythonTests")]
[assembly: InternalsVisibleTo("DynamoCoreWpfTests")]
[assembly: InternalsVisibleTo("IronPythonExtension")]

// Needed to use internal MigrationAssistantRequested event 
[assembly: InternalsVisibleTo("PythonNodeModelsWpf")]
[assembly: InternalsVisibleTo("PythonMigrationViewExtension")]
[assembly: InternalsVisibleTo("DynamoCoreWpf")]
[assembly: TypeForwardedTo(typeof(PythonNodeModels.PythonEngineVersion))]

