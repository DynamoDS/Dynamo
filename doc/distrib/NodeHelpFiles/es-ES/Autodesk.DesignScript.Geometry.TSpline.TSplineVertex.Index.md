## In-Depth
`TSplineVertex.Index` devuelve el número de índice del vértice seleccionado en la superficie de T-Spline. Tenga en cuenta que en una topología de superficie de T-Spline, los índices de cara, arista y vértice no coinciden necesariamente con el número de secuencia del elemento de la lista. Utilice el nodo `TSplineSurface.CompressIndices` para solucionar este problema.

En el ejemplo siguiente, `TSplineTopology.StarPointVertices` se utiliza en una primitiva de T-Spline con la forma de un cuadro. A continuación, se usa `TSplineVertex.Index` para consultar los índices de los vértices de punto de estrella y `TSplineTopology.VertexByIndex` devuelve los vértices seleccionados para su posterior edición.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
