## In Depth
Curvature At Parameter uses U and V input parameters and returns a coordinate system based on the normal, U direction, and V direction at the UV position on the surface. The Normal vector determines the z-axis, while the U and V directions determine the direction of the X and Y axes. The length of the axes are determined by the U and V curvature. In the example below, we first create a surface by using a BySweep2Rails. We then use two number sliders to determine the U and V parameters to create a Coordinate System with a CurvatureAtParameter node.
___
## Example File

![CurvatureAtParameter](./Autodesk.DesignScript.Geometry.Surface.CurvatureAtParameter_img.jpg)

