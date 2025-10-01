## In Depth
Curve by Parameter Line On Surface will create a line along a surface between two input UV coordinates. n the example below, we first create grid of points, and translate them in the Z-direction by a random amount. These points are used to create surface by using a NurbsSurface.ByPoints node. This surface is used as the baseSurface of a ByParameterLineOnSurface node. A set of number sliders are used to adjust the U and V inputs of two UV.ByCoordinates nodes, which are then used to determing the start and end point of the line on the surface.
___
## Example File

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

