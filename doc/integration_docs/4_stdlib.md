
## !!WIP!!

## Dynamo Standard Library Overview

The Standard Library is an effort to bundle more node content with Dynamo Core without expanding the core itself by leveraging the dynamo package loading functionality implemented by the `PackageLoader` and `PackageManager` extension.

In this doc we'll interchangeably use the terms Standard Library, Dynamo Standard Library, std.lib to mean the same thing.

### Considerations / SLA
* SignedPackages only.
* Every effort should be made to avoid breaking changes in these packages.
* High level of polish: icons, node docs, localized content.
* ... others TBD


### Standard Library versus Host Integration Specific Packages

We are intending the `std.lib` to be a core feature, a set of packages that all users gain access to, even if they do not have access to the package manager. The underlying mechanism to support this feature is an additional default loading location for packages directly in the dynamo core directory - relative to DynamoCore.dll.

With some constraints this location will be useable for ADSK Dynamo clients and integrators to ship integration specific packages.

Because the underlying loading mechanism is the same - it will be necessary to make sure that packages included this way do not lead to user confusion about core `std.lib` packages, and integration specific packages that are only available in a single host product. We advise that until the Dynamo team designs and implements apis to support this - that to avoid user confusion - the `std.lib` should only be used in discussion with the team.



### Package Localization

Because packages included in the `std.lib` will be available to more customers and the guarantees we make about them will be stricter (see above) they should be localized.

For internal ADSK packages intended for std.lib inclusion - the current limitations of not being able to publish localized content to the package manager are not blockers as the packages don't necessarily need to be published to the package manager.

Using a workaround it's possible to manually create (and even publish) packages with culture subdirectories in the /bin folder of a package.

Create the culture specific subdirectories you require under the packages's `/bin` folder manually. 

If for some reason, the package needs to also be published to the package manager then you must first publish a version of the package that is missing these culture subfolders - then publish a new version of the package using the DynamoUI `publish package version`. The new version upload in Dynamo should not delete your the folders and files under`/bin` which you have added manually using the windows file explorer. The package upload process in Dynamo will be updated to deal with the requirements for localized files in the future.

These culture subdirectories are loaded without issue by the .net runtime.

For more information on resource assemblies and .resx files please see: https://docs.microsoft.com/en-us/dotnet/framework/resources/creating-resource-files-for-desktop-apps.

You'll likely be creating the `.resx` files and compiling them with visual studio. For a given assembly `xyz.dll` - the resulting resources will be compiled to a new assembly `xyz.resources.dll` - as is described above the location and name of this assembly are important.

The generated `xyz.resources.dll` should be located as follows:
`package\bin\culture\xyz.resources.dll`.

To access the localized strings in your package - you can use the ResourceManager - but even simpler you should be able to refer to the `Properties.Resources.YourLocalizedResourceName` from within the assembly you have added a `.resx` file for. For example, see: 

https://github.com/DynamoDS/Dynamo/blob/master/src/Libraries/CoreNodes/List.cs#L457 for an example of a localized error message

or https://github.com/DynamoDS/Dynamo/blob/master/src/Libraries/CoreNodeModels/ColorRange.cs#L19 for an example of a localized Dynamo specific NodeDescription Attribute string.

or https://github.com/DynamoDS/DynamoSamples/blob/master/src/SampleLibraryUI/Examples/LocalizedCustomNodeModel.cs for another example.

