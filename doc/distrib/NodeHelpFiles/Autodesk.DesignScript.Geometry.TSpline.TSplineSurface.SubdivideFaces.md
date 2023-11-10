## In Depth
In the example below, a T-Spline surface is generated through the `TSplineSurface.ByBoxLengths` node.
A face is selected using `TSplineTopology.FaceByIndex` node and is subdivided using the `TSplineSurface.SubdivideFaces` node.
This node divides the specified faces into smaller faces - four for regular faces, three, five or more  NGons.
When the Boolean input for `exact` is set to true, the result is a surface that attempts to maintain the exact same shape as the original while adding the subdivision. More isocurves may be added to preserve the shape. When set to false, the node only subdivides the one selected face, which often results in a surface that's distinct from the original.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the center of the face to subdivide.
___
## Example File

![TSplineSurface.SubdivideFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces_img.jpg)