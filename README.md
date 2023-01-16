# Dynamo

![mono_build](https://github.com/DynamoDS/Dynamo/workflows/mono_build/badge.svg)
![Dynamo-VS2022Build](https://github.com/DynamoDS/Dynamo/workflows/Dynamo-VS2022Build/badge.svg)

[![Nuget](https://img.shields.io/nuget/v/DynamoVisualProgramming.Core?logo=nuget)](https://www.nuget.org/packages/DynamoVisualProgramming.Core)
[![DynamoVisualProgramming.Core on fuget.org](https://www.fuget.org/packages/DynamoVisualProgramming.Core/badge.svg)](https://www.fuget.org/packages/DynamoVisualProgramming.Core)

![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png)
Dynamo is a visual programming tool that aims to be accessible to both non-programmers and programmers alike. It gives users the ability to visually script behavior, define custom pieces of logic, and script using various textual programming languages.

## Get Dynamo

Looking to learn or download Dynamo? Check out [dynamobim.org](https://dynamobim.org/)!

## Develop

### Create a Node Library for Dynamo

If you're interested in developing a Node library for Dynamo, the easiest place to start is by browsing the [DynamoSamples](https://github.com/DynamoDS/DynamoSamples).
These samples use the [Dynamo NuGet packages](https://www.nuget.org/packages?q=DynamoVisualProgramming) which can be installed using the NuGet package manager in Visual Studio.

[Documentation of the Dynamo API via Fuget.org](https://www.fuget.org/packages/DynamoVisualProgramming.Core/) with a searchable index of public API calls for core functionality in the dynamo nuget packages. *WIP*.

The [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes) document explains changes made to the Dynamo API with every version.

You can learn more about developing libraries for Dynamo on the [Dynamo wiki](https://github.com/DynamoDS/Dynamo/wiki/Zero-Touch-Plugin-Development) or the [Developer page](https://developer.dynamobim.org/).

### Build Dynamo from Source

You will need the following to build the latest Dynamo on Windows:

- [Microsoft Visual Studio 2022](https://visualstudio.microsoft.com/downloads/) (any edition)
- Microsoft .NET Framework 4.8 (included with Visual Studio 2022)
- Node.js v16 and npm v8
- [GitHub for Windows](https://windows.github.com/)
- For runnning Dynamo tests within Visual Studio -[NUnit Test Adapter 2](https://marketplace.visualstudio.com/items?itemName=NUnitDevelopers.NUnitTestAdapter)

If you are working on legacy branches, you may need to install legacy .NET Framework versions through Visual Studio `Tools > Get Tools and Features...` or downloading from [the archive here](https://www.microsoft.com/net/download/archives).

The Dynamo user interface is Windows-only, but with some extra effort the Dynamo engine can be built for other platforms. [Directions for building Dynamo on Linux or OS X can be found here](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-on-Linux,-Mac).
Find more about how to build Dynamo at our [wiki](https://github.com/DynamoDS/Dynamo/wiki) and [Dynamo Developer Resources](https://developer.dynamobim.org/)

## Contribute

Dynamo is an open-source project and would be nothing without its community.  You can make suggestions or track and submit bugs via [Github issues](https://github.com/DynamoDS/Dynamo/issues).  You can submit your own code to the Dynamo project via a Github [pull request](https://github.com/DynamoDS/Dynamo/blob/master/CONTRIBUTING.md).

## Releases

See the [Release Notes](https://github.com/DynamoDS/Dynamo/wiki/Release-Notes).

## Instrumentation

Dynamo now leverages the same instrumentation component as other Autodesk products which share the opt-in option. The data will be used to enhance the usability of the product.

## License

Dynamo is licensed under the Apache License. Dynamo also uses a number of third party libraries, some with different licenses. You can find more information [here](LICENSE.txt).
