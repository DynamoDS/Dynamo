## In-Depth
`TSplineEdge.Info` returns the following properties of a T-Spline surface edge: 
- `uvnFrame`: point on the hull, U vector, V vector, and normal vector of the T-Spline Edge
- `index`: the index of the Edge
- `isBorder`: whether the chosen Edge is a Border of T-Spline surface
- `isManifold`; whether the chosen Edge is Manifold

In the example below, `TSplineTopology.DecomposedEdges` is used to obtain a list of all edges of a T-Spline cylinder primitive surface, and `TSplineEdge.Info` is used to investigate their properties.


## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)