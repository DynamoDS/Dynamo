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
[assembly: Guid("619AFC1C-FC8E-4F1B-ADE9-AE6FBDB3CC38")]
[assembly: RequiresThread(ApartmentState.STA)]
[assembly: InternalsVisibleTo("WpfVisualizationTests")]
[assembly: InternalsVisibleTo("CrashReportingTests")]
[assembly: TypeForwardedToAttribute(typeof(DynamoCoreWpfTests.Utility.DispatcherUtil))]
