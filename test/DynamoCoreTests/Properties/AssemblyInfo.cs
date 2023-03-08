using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DynamoCoreTests")]
[assembly: AssemblyCulture("")]
#if NET6_0_OR_GREATER
[assembly: Apartment(ApartmentState.STA)]
#elif NETFRAMEWORK
        [assembly: RequiresSTA]
#endif
// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1a12b389-dc06-4114-ac45-2e0903cab66d")]
[assembly: InternalsVisibleTo("DynamoCoreTests")]
