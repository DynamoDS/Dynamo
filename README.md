![mono_build](https://github.com/DynamoDS/Dynamo/workflows/mono_build/badge.svg)
![Dynamo-VS2019Build](https://github.com/DynamoDS/Dynamo/workflows/Dynamo-VS2019Build/badge.svg)

![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png)
Dynamo is a visual programming tool that aims to be accessible to both non-programmers and programmers alike. It gives users the ability to visually script behavior, define custom pieces of logic, and script using various textual programming languages.


## Get Dynamo ##

Looking to learn or download Dynamo?  Check out [dynamobim.org](http://dynamobim.org/learn/)!


## Develop ###
### Create a Node Library for Dynamo ###
If you're interested in developing a Node library for Dynamo, the easiest place to start is by browsing the [DynamoSamples](https://github.com/DynamoDS/DynamoSamples).  
These samples use the [Dynamo NuGet packages](https://www.nuget.org/packages?q=DynamoVisualProgramming) which can be installed using the NuGet package manager in Visual Studio.

[Documentation of the Dynamo API via Fuget.org]( https://www.fuget.org/packages/DynamoVisualProgramming.Core/2.5.0.7432) with a searchable index of public API calls for core functionality in the dynamo nuget packages. *WIP*. 
#### note: link may change in the near future.

The [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes) document explains changes made to the Dynamo API with every version.

You can learn more about developing libraries for Dynamo on the [Dynamo wiki](https://github.com/DynamoDS/Dynamo/wiki/Zero-Touch-Plugin-Development) or the [Developer page](http://developer.dynamobim.org/).

### Build Dynamo from Source ###
You will need the following to build Dynamo `master` branch:

- Microsoft Visual Studio 2017
- Microsoft .NET Framework 4.7.
- [GitHub for Windows](https://windows.github.com/)
- Microsoft DirectX (install from %GitHub%\Dynamo\tools\install\Extra\DirectX\DXSETUP.exe)

If you are working on legacy branches, you need to install legacy .Net Framework archives [here](https://www.microsoft.com/net/download/archives).

Directions for building Dynamo on other platforms (e.g. Linux or OS X) can be found [here](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-on-Linux,-Mac).  

Find more about how to build Dynamo at our [wiki](https://github.com/DynamoDS/Dynamo/wiki).


## Contribute ##

Dynamo is an open-source project and would be nothing without its community.  You can make suggestions or track and submit bugs via [Github issues](https://github.com/DynamoDS/Dynamo/issues).  You can submit your own code to the Dynamo project via a Github [pull request](https://github.com/DynamoDS/Dynamo/blob/master/CONTRIBUTING.md).


## Releases ##

See the [Release Notes](https://github.com/DynamoDS/Dynamo/wiki/Release-Notes).

## Instrumentation ##
Dynamo now leverage the same instrumentation component as other Autodesk products which share the opt-in option. Those data will be used to enhance the usability of the product.



## License ##

Dynamo is licensed under the Apache License. Dynamo also uses a number of third party libraries, some with different licenses. You can find more information [here](LICENSE.txt).

