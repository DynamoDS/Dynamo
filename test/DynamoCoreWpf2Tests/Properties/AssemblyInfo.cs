using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("DynamoCoreWpfTests")]
[assembly: AssemblyCulture("")]
[assembly: Guid("D481E816-AE9A-434F-B646-C6F426CF1B32")]
[assembly: RequiresThread(ApartmentState.STA)]
[assembly: InternalsVisibleTo("WpfVisualizationTests")]
[assembly: InternalsVisibleTo("CrashReportingTests")]
[assembly: TypeForwardedToAttribute(typeof(DynamoCoreWpfTests.Utility.DispatcherUtil))]
