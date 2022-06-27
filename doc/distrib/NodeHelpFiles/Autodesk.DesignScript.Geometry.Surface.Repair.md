## In Depth
Repair will attempt to repair surfaces or polysurfaces, which have invalid geometry, as well as potentially performing optimizations. The repair node will return a new surface object. 
This node is useful when you encounter errors performing operations on imported or converted geometry.

For example, if you import data from a host context like **Revit** or from a **.SAT** file, and you find that it unexpectedly fails to boolean or trim, you may find a repair operation cleans up any *invalid geometry* that is causing the failure.

In general, you should not need to use this functionality on geometry you create in Dynamo, only on geometry from external sources. If you find that is not the case, please report a bug to the Dynamo team github!
___


