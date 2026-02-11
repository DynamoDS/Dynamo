## In-Depth
Tenga en cuenta que, en una topología de superficie de T-Spline, los índices de `Face`, `Edge` y `Vertex` no coinciden necesariamente con el número de secuencia del elemento de la lista. Utilice el nodo `TSplineSurface.CompressIndices` para solucionar este problema.

En el ejemplo siguiente, se utiliza `TSplineTopology.DecomposedEdges` para recuperar las aristas de borde de una superficie de T-Spline y, a continuación, se usa un nodo `TSplineEdge.Index` para obtener los índices de las aristas especificadas.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
