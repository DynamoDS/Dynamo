## In Depth
`Surface.TrimWithEdgeLoops` trims the surface with a collection of one or more closed PolyCurves that must all lie on the surface within the specified tolerance. If one or more holes need to be trimmed from the input surface, there must be one outer loop specified for the boundary of the surface and one inner loop for each hole. If the region between the surface boundary and the hole(s) needs to be trimmed, only the loop for each hole should be provided. For a periodic surface with no outer loop such as a spherical surface, the trimmed region can be controlled by reversing the direction of the loop curve.

The tolerance is the tolerance used when deciding whether curve ends are coincident and whether a curve and surface are coincident. The supplied tolerance cannot be smaller than any of the tolerances used in the creation of the input PolyCurves. The default value of 0.0 means that the largest tolerance used in the creation of the input PolyCurves will be used.

In the example below, two loops are trimmed out of a surface, returning two new surfaces highlighted in blue. The number slider adjusts the shape of the new surfaces. 

___
## Example File

![Surface.TrimWithEdgeLoops](./Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface,%20loops,%20tolerance)_img.jpg)