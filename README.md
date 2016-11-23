![Image](https://ci.appveyor.com/api/projects/status/github/DynamoDS/Dynamo?branch=master) ![Image](https://travis-ci.org/DynamoDS/Dynamo.svg?branch=master)

![Image](https://raw.github.com/ikeough/Dynamo/master/doc/distrib/Images/dynamo_logo_dark.png)
Dynamo is a visual programming tool that aims to be accessible to both non-programmers and programmers alike. It gives users the ability to visually script behavior, define custom pieces of logic, and script using various textual programming languages.


## Get Dynamo ##

Looking to learn or download Dynamo?  Check out [dynamobim.org](http://dynamobim.org/learn/)!


## Develop ###
### Create a Node Library for Dynamo ###
If you're interested in developing a Node library for Dynamo, the easiest place to start is by browsing the [DynamoSamples](https://github.com/DynamoDS/DynamoSamples).  
These samples use the [Dynamo NuGet packages](https://www.nuget.org/packages?q=DynamoVisualProgramming) which can be installed using the NuGet package manager in Visual Studio.

[Documentation of the Dynamo API]( http://dynamods.github.io/DynamoAPI) with a searchable index of public API calls for core functionality. This will be expanded to include regular nodes and Revit functionality.

The [API Changes](https://github.com/DynamoDS/Dynamo/wiki/API-Changes) document explains changes made to the Dynamo API with every version.

You can learn more about developing libraries for Dynamo on the [Dynamo wiki](https://github.com/DynamoDS/Dynamo/wiki/Zero-Touch-Plugin-Development).

### Build Dynamo from Source ###
You will need the following to build Dynamo:

- Microsoft Visual Studio 2015
- [GitHub for Windows](https://windows.github.com/)
- Microsoft .NET Framework 4.5.
- Microsoft DirectX (install from %GitHub%\Dynamo\tools\install\Extra\DirectX\DXSETUP.exe)

Directions for building Dynamo on other platforms (e.g. Linux or OS X) can be found [here](https://github.com/DynamoDS/Dynamo/wiki/Dynamo-on-Linux,-Mac).  

Find more about how to build Dynamo at our [wiki](https://github.com/DynamoDS/Dynamo/wiki).


## Contribute ##

Dynamo is an open-source project and would be nothing without its community.  You can make suggestions or track and submit bugs via [Github issues](https://github.com/DynamoDS/Dynamo/issues).  You can submit your own code to the Dynamo project via a Github [pull request](https://help.github.com/articles/using-pull-requests).


## Releases ##

See the [Release Notes](https://github.com/DynamoDS/Dynamo/wiki/Release-Notes).

## Instrumentation ##
Dynamo contains an instrumentation system that anonymously reports usage data to the Dynamo team. This data will be used to enhance the usability of the product. Aggregated summaries of the data will be shared back with the Dynamo community.

An example of the data communicated is:

"DateTime: 2013-08-22 19:17:21, AppIdent: Dynamo, Tag: Heartbeat-Uptime-s, Data: MTMxMjQxLjY3MzAyMDg=, Priority: Info, SessionID: 3fd39f21-1c3f-4cf3-8cdd-f46ca5dde636, UserID: 2ac95f29-a912-49a8-8fb5-e2d287683d94"

The Data is Base64 encoded. For example, the data field above ('MTMxMjQxLjY3MzAyMDg=') decodes to: '131241.6730208' This represents the number of seconds that the instance of Dynamo has been running.

The UserID is randomly generated when the application is first run. The SessionID is randomly generated each time Dynamo is opened.

## License ##

Dynamo is licensed under the Apache License. Dynamo also uses a number of third party libraries, some with different licenses. You can find more information [here](LICENSE.txt).

