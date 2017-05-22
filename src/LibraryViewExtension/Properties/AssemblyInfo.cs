using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LibraryViewExtension")]

[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("0b32f8ed-5d7a-475e-8536-7971516165fa")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.ExternalAssembly, //where theme specific resource dictionaries are located
                                                 //(used if a resource is not found in the page, 
                                                 // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page, 
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: InternalsVisibleTo("ViewExtensionLibraryTests")]
