## In Depth
In the example below, all inner vertices of a T-Spline plane surface are collected using the `TSplineTopology.InnerVertices` node. The vertices, along with the surface that they belong to, are used as input for the `TSplineSurface.PullVertices` node. The `geometry` input is a sphere located above the plane surface. The `surfacePoints` input is set to false and control points are used to perform the pull operation. 
___
## Example File

![TSplineSurface.PullVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.PullVertices_img.jpg)