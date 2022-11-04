
## !!WIP!!

## Dynamo Built-In Packages Overview

The Built-In Packages mechanism is an effort to bundle more node content with Dynamo Core without expanding the core itself by leveraging the dynamo package loading functionality implemented by the `PackageLoader` and `PackageManager` extension.

In this doc we'll interchangeably use the terms Built-In Packages, Dynamo Built-In Packages, builtin packages to mean the same thing.

### Should I ship a package as a Built-In Package?
* The package must have signed binary entry points or it will not be loaded.
* Every effort should be made to avoid breaking changes in these packages. This means the package content should have automated tests.
* Semantic versioning, it's probably a good idea to version your package using a semantic versioning scheme, and to communicate it to users in the package description or docs.
* Automated tests! Please see above, if a package is included using the Built-In Package mechanism, to a user it appears to be part of the product and should be tested like a product.
* High level of polish: icons, node docs, localized content.
* Don't ship packages that you or your team cannot maintain.
* Don't ship third party packages this way (see above).

Basically, you should have full control over the package, the ability to fix it, keep it updated, and to test it against the latest changes in Dynamo and your product. You also need the ability to sign it.


### Built-In Packages versus Host Integration Specific Packages

We are intending the `Built-In Packages` to be a core feature, a set of packages that all users gain access to, even if they do not have access to the package manager. Currently the underlying mechanism to support this feature is an additional default loading location for packages directly in the dynamo core directory - relative to DynamoCore.dll.

With some constraints this location will be useable for ADSK Dynamo clients and integrators to distribute their integration specific packages. *(for example, the Dynamo Formit integration requires a custom Dynamo Formit package).*

Because the underlying loading mechanism is the same for both core and host specific packages - it will be necessary to make sure that packages distributed this way do not lead to user confusion about core `Built-In Packages` packages vs. integration specific packages that are only available in a single host product. We advise that that to avoid user confusion host-specific packages should be introduced in discussion with the Dynamo teams.


### Package Localization

Because packages included in the `Built-In Packages` will be available to more customers and the guarantees we make about them will be stricter (see above) they should be localized.

For internal ADSK packages intended for `Built-In Packages` inclusion - the current limitations of not being able to publish localized content to the package manager are not blockers as the packages don't necessarily need to be published to the package manager.

Using a workaround it's possible to manually create (and even publish) packages with culture subdirectories in the /bin folder of a package.

First create the culture specific subdirectories you require under the packages's `/bin` folder manually. 

If for some reason, the package needs to also be published to the package manager then you must first publish a version of the package that is missing these culture subdirectories - then publish a new version of the package using the DynamoUI `publish package version`. The new version upload in Dynamo should not delete your folders and files under`/bin`, which you have added manually using the windows file explorer. The package upload process in Dynamo will be updated to deal with the requirements for localized files in the future.

These culture subdirectories are loaded without issue by the .net runtime if they are located in the same directory as the node / extension binaries.

For more information on resource assemblies and .resx files please see: https://docs.microsoft.com/en-us/dotnet/framework/resources/creating-resource-files-for-desktop-apps.

You'll likely be creating the `.resx` files and compiling them with visual studio. For a given assembly `xyz.dll` - the resulting resources will be compiled to a new assembly `xyz.resources.dll` - as is described above the location and name of this assembly are important.

The generated `xyz.resources.dll` should be located as follows:
`package\bin\culture\xyz.resources.dll`.

To access the localized strings in your package - you can use the ResourceManager - but even simpler you should be able to refer to the `Properties.Resources.YourLocalizedResourceName` from within the assembly you have added a `.resx` file for. For example, see: 

https://github.com/DynamoDS/Dynamo/blob/master/src/Libraries/CoreNodes/List.cs#L457 for an example of a localized error message

or https://github.com/DynamoDS/Dynamo/blob/master/src/Libraries/CoreNodeModels/ColorRange.cs#L19 for an example of a localized Dynamo specific NodeDescription Attribute string.

or https://github.com/DynamoDS/DynamoSamples/blob/master/src/SampleLibraryUI/Examples/LocalizedCustomNodeModel.cs for another example.

### Node Library Layout

Normally, when Dynamo loads nodes from a package, it places them in the `Addons` section in the node library. To better integrate built-in package nodes with other built-in content, we've added the ability for built-in package authors to supply a partial `layout specification` file to help place the new nodes into the correct top level category in the `default` library section.

For example, the following layout spec json file, if found at the path `package/extra/layoutspecs.json` will place the nodes specified by `path` into the `Revit` category in the `default` section which is the main built-in nodes section.

note that nodes imported from a builtin package will have the prefix `bltinpkg://` when they are considered for matching against a path included in the layout spec.

```json
{
  "sections": [
    {
      "text": "default",
      "iconUrl": "",
      "elementType": "section",
      "showHeader": false,
      "include": [ ],
      "childElements": [
        {
          "text": "Revit",
          "iconUrl": "",
          "elementType": "category",
          "include": [],
          "childElements": [
            {
              "text": "some sub group name",
              "iconUrl": "",
              "elementType": "group",
              "include": [
                {
                  "path": "bltinpkg://namespace.namespace",
                  "inclusive": false
                }
              ],
              "childElements": []
            }
          ]
        }
      ]
    }
  ]
}
```

Complex layout modifications are not well tested or supported, the intention for this particular layout specification loading is to move an entire package namespace under a particular host category like `Revit` or `Formit`. 
