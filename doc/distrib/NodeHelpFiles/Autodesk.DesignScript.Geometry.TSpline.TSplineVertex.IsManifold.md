## In-Depth
In the example below, a non-manifold surface is produced by joining two surfaces that share an internal edge. A result is a surface that has no clear front and back. The non-manifold surface can only be displayed in box mode until it is repaired. `TSplineTopology.DecomposedVertices` is used to query all the vertices of the surface and `TSplineVertex.IsManifold` node is usedto highlight which of the vertices qualify as manifold. The non-manifold vertices are extracted and their position visualized by using the `TSplineVertex.UVNFrame` and `TSplineUVNFrame.Position` nodes.


## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold_img.jpg)