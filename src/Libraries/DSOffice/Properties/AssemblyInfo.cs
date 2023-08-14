using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DSOffice")]
[assembly: AssemblyCulture("")]

[assembly: InternalsVisibleTo("DynamoMSOfficeTests")]

#if _WINDOWS
    [assembly: TypeForwardedTo(typeof(DSOffice.WorkBook))]
    [assembly: TypeForwardedTo(typeof(DSOffice.WorkSheet))]
#endif
