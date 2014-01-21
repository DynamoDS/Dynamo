Author: peter.boyer@autodesk.com
Last updated: 1/21/2014

How do the LibG dll's get copied correctly?

The LibG dll's are copied as a post-build event of DynamoCore.

Where should they go?

The LibG dll's - including the ASM dll's located located in Common - should NOT end up amongst Dynamo's core Dll's in the install.  This will cause them to be scanned prematurely by Dynamo.  They should all be located in the dll folder.  ProtoGeometry.config identifies the entry point for LibG - it is located in extern/DesignScript.  If you locate ANY of the LibG.*.dll assemblies amongst the rest of Dynamo's assemblies, you may have issue loading LibG at runtime.

Where is ProtoGeometry.config?

It is in extern/DesignScript.  There should be no other copy in the repo.  It is copied along with the assembly to the bin directory.





