using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MIConvexHull")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("UT-ADLab")]
[assembly: AssemblyProduct("MIConvexHull")]
[assembly: AssemblyCopyright("Copyright © UT-ADLab 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Needed to create and apply a strong name (a crypto hash key) to allow registering this dll in the GAC and then referencing the assembly in Dynamo. 
// This is to get around the failure we were seeing trying to reference this pre-compiled dll from dyanmo\packages directory.
// see http://support.microsoft.com/kb/315682

[assembly: AssemblyKeyFile("..\\..\\..\\lib\\GACKey.snk")]


// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d24b97d6-7b40-4bb5-b831-806d40106a83")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.10.1021")]
[assembly: AssemblyFileVersion("1.0.10.1021")]
