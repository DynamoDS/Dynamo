## In-Depth
`TSplineVertex.Info` returns the following properties of a T-Spline vertex: 
- `uvnFrame`: point on the hull, U vector, V vector, and normal vector of the T-Spline Vertex
- `index`: the index of the chosen vertex on the T-Spline Surface
- `isStarPoint`: whether the chosen vertex is a star point
- `isTpoint`: whether the chosen vertex is a T-Point
- `isManifold`: whether the chosen vertex is Manifold
- `valence`: number of edges on the chosen T-Spline vertex
- `functionalValence`: the functional valence of a vertex. See documentation for `TSplineVertex.FunctionalValence` node for more. 

In the example below, `TSplineSurface.ByBoxCorners` and `TSplineTopology.VertexByIndex` are used to respectively create a T-Spline surface and select its vertices. `TSplineVertex.Info` is used to collect the above information about a chosen vertex.

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)