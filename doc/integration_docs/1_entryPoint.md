## Dynamo Custom Entry Point: 
#### Dynamo Revit as Example 

https://github.com/DynamoDS/DynamoRevit/blob/f1e305e3180819815d8adc763976eadb95831ebe/src/DynamoRevit/DynamoRevit.cs#L425 

The `DynamoModel` is the entry point for an application hosting Dynamo – it represents a Dynamo Application. The model is the top level root object which contains references to the other important data structures and objects that make up the dynamo application and DesignScript virtual machine. 

A configuration object is used to set common parameters on the `DynamoModel` when it is constructed. 

The examples in this document are taken from the DynamoRevit implementation, which is an integration where Revit hosts a `DynamoModel` as an Addin. (Plugin architecture for Revit). When this Addin loads – it starts a `DynamoModel` and then displays it to the user with a `DynamoView` and `DynamoViewModel`. 

Dynamo is a c# .net project and to use it in process in your application you need to be able to host and execute .net code.

DynamoCore is a cross platform compute engine and collection of core models, which can be built with .net or mono (in the future .net core).
But DynamoCoreWPF contains the windows only UI components of Dynamo and will not compile on other platforms.

### Steps to Customize your Dynamo Entry Point 

To Initialize the `DynamoModel`, integrators will need to do these steps from somewhere in the host's code.

### Preload shared Dynamo Dlls from host.  

Currently the list in D4R only includes `Revit\SDA\bin\ICSharpCode.AvalonEdit.dll.` This is done to avoid library version conflicts between Dynamo and Revit. E.g. When conflicts on `AvalonEdit` happen, the function of code block can be totally broken. The issue is reported in Dynamo 1.1.x at https://github.com/DynamoDS/Dynamo/issues/7130 and, also manually reproducible. If integrators found library conflicts between host function and Dynamo, it is suggested to do this as a first step. This is sometimes required to stop other plugin or the host application itself from loading an incompatible version of as shared dependency. A better solution is to resolve the version conflict by aligning the version - or to use a .net binding redirect in the host’s app.config if possible. 

### Initialize UpdateManager 

The UpdateManager component checks for the dynamo product updates by requesting update version info from configured download source path (AWS S3 bucket link). It skips the update if the user’s local version is newer than the version online.

To create an `updateManager` object, first the users disk is searched for a config - in the file named UpdateManagerConfig.xml, and created if it does not exist using `UpdateManagerConfiguration` default constructor with default values. Then an `UpdateManager` is created using that config.  

The config looks like:
``` xml
<UpdateManagerConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"> 

<DownloadSourcePath>http://dyn-builds-data.s3.amazonaws.com/</DownloadSourcePath> 

<SignatureSourcePath>http://dyn-builds-data-sig.s3.amazonaws.com/</SignatureSourcePath> 

<CheckNewerDailyBuild>false</CheckNewerDailyBuild> 

<ForceUpdate>false</ForceUpdate> 

<InstallerNameBase>DynamoInstall</InstallerNameBase> 

<DisableUpdates>false</DisableUpdates> 

</UpdateManagerConfiguration> 
```
 
Although `UpdateManager` has not been obsoleted in code at this time, it has been disabled in a different way after Dynamo 2.1.x in DynamoRevit since  
Dynamo installers are no longer delivered to the AWS S3 bucket as Dynamo releases. 

Dynamo deliveries are now named starting with DynamoCoreRuntime instead of DynamoInstall,and the build scan code will no longer work to indicate the latest build for users since the InstallerNameBase is still DynamoInstall. It also no longer makes sense with how Dynamo Revit is delivered at this time.

If you would like to disable automatic Dynamo update for your integration, please follow the example by Revit team here to set DisableUpdates to false under UpdateManager.Configuration. 

``` c#
if(revitUpdateManager.Configuration is IDisableUpdateConfig) 

       (revitUpdateManager.Configuration as IDisableUpdateConfig).DisableUpdates=true; 
```
 

### Loading ASM 

#### What are ASM and LibG

ASM is the ADSK geometry library that Dynamo's is built on top of.

LibG is .Net user-friendly wrapper around ASM geometry kernel. libG shares its versioning scheme with ASM - it is using the same major and minor version number of ASM to indicate it is the corresponding wrapper of particular ASM version. When given an ASM version, the corresponding libG version should be the same. LibG in most cases should work with all versions of ASM of a particular major version. For example - LibG 223 - should be able to load any ASM 223 version.


#### Dynamo Sandbox loading ASM 

Dynamo Sandbox is designed to be able to work with multiple ASM versions, to accomplish this, multiple libG versions are bundled and shipped with the core. There is built-in functionality in Dynamo Shape Manager to search for Autodesk products which are shipped with ASM, so Dynamo can load ASM from these products and have geometry nodes work without explicitly being loaded into a host application. The products list as it exists today is: 
```
private static readonly List<string> ProductsWithASM = new List<string>() 

 { "Revit", "Civil", "Robot Structural Analysis", "FormIt" }; 
```
Dynamo will search the windows registry and find if the Autodesk products in this list are installed on the user’s machine, if any of these are installed, then it will search for ASM binaries, and will get the version and look for a corresponding libG version in Dynamo.  

Given the ASM version, the following ShapeManager API will pick the corresponding libG preloader location to load. If there is an exact version match it will be used, otherwise the closest versioned libG below, but with the same major version will be loaded.  

E.g. If Dynamo is integrated with a Revit dev build where there is a newer ASM build 225.3.0, Dynamo will try to use libG 225.3.0 if it exists, otherwise it will try to use the closest major version less than its first choice, ie 225.0.0. 

`public static string GetLibGPreloaderLocation(Version asmVersion, string dynRootFolder)` 

#### Dynamo in-process integration loading ASM from host 

Revit is  the first entry in the ASM product search list, which means by default `DynamoSandbox.exe` will try to load ASM from Revit first, we still want to make sure the integrated D4R working session loads ASM from the current Revit host: e.g. if user has both R2018 and R2020 on computer, when launching D4R from R2020, D4R should be using ASM 225 from R2020 instead of ASM 223 from R2018. Integrators will need to implement similar calls to the following to force their specified version to load. 

```
internal static Version PreloadAsmFromRevit() 

{ 

     var asmLocation = AppDomain.CurrentDomain.BaseDirectory; 
     Version libGVersion = findRevitASMVersion(asmLocation); 
     var dynCorePath = DynamoRevitApp.DynamoCorePath; 
     var preloaderLocation = DynamoShapeManager.Utilities.GetLibGPreloaderLocation(libGVersion, dynCorePath); 
     Version preLoadLibGVersion = PreloadLibGVersion(preloaderLocation); 
     DynamoShapeManager.Utilities.PreloadAsmFromPath(preloaderLocation, asmLocation); 
     return preLoadLibGVersion; 

} 
```

#### Dynamo loading ASM from a customized path 

Recently we have added the ability for `DynamoSandbox.exe` and `DynamoCLI.exe` to load a particular ASM version. To skip the normal registry search behavior, you can use the `–gp` flag to force Dynamo to load ASM from a particular path. 

`DynamoSandbox.exe -gp “somePath/To/ASMDirectory/” `

  



### Create A StartConfiguration 

The StartupConfiguration is used to pass in as a param for initializing DynamoModel which indicates that it contains almost all the definitions for how you would like to customize your Dynamo session settings. Depending on how the following properties are set, the Dynamo integration could vary between different integrators. E.g. different integrators could set different python template paths or number formats displayed. 

It consists of the following: 

* DynamoCorePath // Where the loading DynamoCore binaries are located

* DynamoHostPath // Where the Dynamo integration binaries are located

* GeometryFactoryPath // Where loaded libG binaries are located

* PathResolver //object that helps to resolve various files

* PreloadLibraryPaths // where are preloaded nodes binaries located, e.g. DSOffice.dll 

* AdditioanlNodeDirectories // Where additional node binaries are located
 
* AdditionalResolutionPaths // Additional assembly resolution paths for other dependencies that might be required while loading libraries 

* UserDataRootFolder // User data folder, e.g. `"AppData\Roaming\Dynamo\Dynamo Revit"` 

* CommonDataRootFolder // Default folder for saving custom definitions, samples, gallery etc.

* Context // Integrator host name + version `(Revit<BuildNum>)`

* SchedulerThread // Integrator scheduler thread implementing `ISchedulerThread` - for most integrators this is the Main UI thread or from whatever thread they can access their API.

* StartInTestMode // Whether the current session is a test automation session - modifies a bunch of Dynamo behavior - don't use unless you are writing tests.

* AuthProvider // Integrator’s implementation of IAuthProvider, e.g. RevitOxygenProvider implementation is in Greg.dll - Used for packageManager upload integration. 

### Preferences 

Default preference setting path is managed by `PathManager.PreferenceFilePath`, e.g. `"AppData\\Roaming\\Dynamo\\Dynamo Revit\\2.5\\DynamoSettings.xml"`. Integrators can decide if they would like to also ship a customized preference setting file to a location which needs to be aligned with path manager. The following are preference setting properties which are serialized: 

* IsFirstRun // Indicates if it is the first time running this version of dynamo, e.g. used to determine if need to display GA opt-in/out message. Also used to determine if it’s needed to migrate the legacy Dynamo preference setting when launching a new Dynamo version, so users have consistent experience 

* IsUsageReportingApproved // Indicates whether usage reporting is approved or not 

* IsAnalyticsReportingApproved // Indicates whether analytics reporting is approved or not 

* LibraryWidth // The width of the Dynamo left library panel. 

* ConsoleHeight // The height of the console display 

* ShowPreviewBubbles // Indicates if preview bubbles should be displayed 

* ShowConnector // Indicates if connectors are displayed 

* ConnectorType //Indicates the type of connector: Bezier or Polyline 

* BackgroundPreviews // Indicates active state of the specified background preview 

* RenderPrecision // The level of render precision - lower generates meshes with less triangles. Higher will generate smoother geometry in the background preview. 128 is a good fast number for preview geometry.

* ShowEdges // Indicates whether surface and solid edges will be rendered 

* ShowDetailedLayout // UNUSED

* WindowX, WindowY // Last X, Y coordinate of the Dynamo window 

* WindowW, WindowH // Last width, height of the Dynamo window 

* UseHardwareAcceleration // Should Dynamo use hardware acceleration if it is supported 

* NumberFormat // The decimal precision used to display numbers in preview bubble toString(). 

* MaxNumRecentFiles // The maximum number of recent file paths to be saved 

* RecentFiles // A list of recently opened file paths, touching this will directly affect the recent files list in Dynamo start up page 

* BackupFiles // A list of backup file paths 

* CustomPackageFolders // A list of folders containing zero-touch binaries and direcory paths that will be scanned for packages and custom nodes.

* PackageDirectoriesToUninstall // A list of packages used by the Package Manager to determine which packages are marked for deletion. These paths will be deleted if possible during Dynamo startup.

* PythonTemplateFilePath // Path to the Python (.py) file to use as a starting template when creating a new PythonScript Node - this can be used to setup a custom python template for your integration.

* BackupInterval // Indicates how long (in milliseconds) will the graph be automatically saved 

* BackupFilesCount // Indicates how many backups will be made 

* PackageDownloadTouAccepted // Indicates if the user has accepted the terms of use for downloading packages from package manager 

* OpenFileInManualExecutionMode // Indicates the default state of the "Open in Manual Mode" checkbox in OpenFileDialog 

* NamespacesToExcludeFromLibrary // Indicates which (if any) namespaces should not be displayed in the Dynamo node library. String format: "[library name]:[fully qualified namespace]" 

An example of serialized preference settings: 

``` xml 
<PreferenceSettings xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"> 

<IsFirstRun>false</IsFirstRun> 

<IsUsageReportingApproved>false</IsUsageReportingApproved> 

<IsAnalyticsReportingApproved>false</IsAnalyticsReportingApproved> 

<LibraryWidth>204</LibraryWidth> 

<ConsoleHeight>0</ConsoleHeight> 

<ShowPreviewBubbles>true</ShowPreviewBubbles> 

<ShowConnector>true</ShowConnector> 

<ConnectorType>BEZIER</ConnectorType> 

<BackgroundPreviews> 

<BackgroundPreviewActiveState> 

<Name>IsBackgroundPreviewActive</Name> 

<IsActive>true</IsActive> 

</BackgroundPreviewActiveState> 

<BackgroundPreviewActiveState> 

<Name>IsRevitBackgroundPreviewActive</Name> 

<IsActive>true</IsActive> 

</BackgroundPreviewActiveState> 

</BackgroundPreviews> 

<IsBackgroundGridVisible>true</IsBackgroundGridVisible> 

<RenderPrecision>128</RenderPrecision> 

<ShowEdges>false</ShowEdges> 

<ShowDetailedLayout>true</ShowDetailedLayout> 

<WindowX>553</WindowX> 

<WindowY>199</WindowY> 

<WindowW>800</WindowW> 

<WindowH>676</WindowH> 

<UseHardwareAcceleration>true</UseHardwareAcceleration> 

<NumberFormat>f3</NumberFormat> 

<MaxNumRecentFiles>10</MaxNumRecentFiles> 

<RecentFiles> 

<string></string> 

</RecentFiles> 

<BackupFiles> 

<string>..AppData\Roaming\Dynamo\Dynamo Revit\backup\backup.DYN</string> 

</BackupFiles> 

<CustomPackageFolders> 

<string>..AppData\Roaming\Dynamo\Dynamo Revit\2.5</string> 

</CustomPackageFolders> 

<PackageDirectoriesToUninstall /> 

<PythonTemplateFilePath /> 

<BackupInterval>60000</BackupInterval> 

<BackupFilesCount>1</BackupFilesCount> 

<PackageDownloadTouAccepted>true</PackageDownloadTouAccepted> 

<OpenFileInManualExecutionMode>false</OpenFileInManualExecutionMode> 

<NamespacesToExcludeFromLibrary> 

<string>ProtoGeometry.dll:Autodesk.DesignScript.Geometry.TSpline</string> 

</NamespacesToExcludeFromLibrary> 

</PreferenceSettings> 
``` 
 

* Extensions // A list of extensions implementing IExtension, if it’s null, Dynamo will load extensions from the default path (`extensions` folder under Dynamo folder) 

* IsHeadless // Indicates if Dynamo is launched without UI, effects Analytics. 

* UpdateManager // Integrator’s implementation of UpdateManager, see description above 

* ProcessMode // Equivalent to TaskProcessMode, Synchronous if in test mode, otherwise Asynchronous - This controls the behavior of the scheduler. Single threaded enviornments may also set this to synchronous.

Use the target StartConfiguration to launch `DynamoModel`

Once the StartConfig is passed to launch `DynamoModel`, DynamoCore will oversee the actual specifics to make sure Dynamo session is initialized correctly with the details specified. There should be some after set-up steps individual integrators will need to do after `DynamoModel` is initialized, e.g. in D4R, events are subscribed to watch for Revit host transactions or document updates, Python Node customization, etc. 

 ### Let's get to the 'visual programming' part already

To Initialize `DynamoViewModel` and `DynamoView`, you'll need to first construct a `DynamoViewModel` - which can be done
using the `DynamoViewModel.Start` static method. See below:

``` c#

    viewModel = DynamoViewModel.Start(
                    new DynamoViewModel.StartConfiguration()
                    {
                        CommandFilePath = commandFilePath,
                        DynamoModel = model,
                        Watch3DViewModel = 
                            HelixWatch3DViewModel.TryCreateHelixWatch3DViewModel(
                                null,
                                new Watch3DViewModelStartupParams(model), 
                                model.Logger),
                        ShowLogin = true
                    });
     
     var view = new DynamoView(viewModel);

```

The `DynamoViewModel.StartConfiguration` has much fewer options than the Model's config. They are mostly self explanatory - the `CommandFilePath` can be ignored unless you are writing a test case.


The `Watch3DViewModel` parameter controls how the background preview and watch3d nodes display 3d geometry. You can use your own implementation if you implement the required interfaces.

To construct the `DynamoView`, all that is required is the `DynamoViewModel`. The View is a window control and can be shown using WPF.

 ### DynamoSandbox.exe example:

 DynamoSandbox.exe is a development environment for testing, using, and experimenting with DynamoCore. It's a great example to checkout to see how `DynamoCore` and `DynamoCoreWPF` components are loaded and setup.
`
 You can see some of the entry point [here](https://github.com/DynamoDS/Dynamo/blob/master/src/DynamoSandbox/DynamoCoreSetup.cs#L37)

