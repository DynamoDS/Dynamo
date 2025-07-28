## In-Depth
Tenere presente che nella topologia di una superficie T-Spline, gli indici di `Face`, `Edge` e `Vertex` non coincidono necessariamente con il numero di sequenza dell'elemento nell'elenco. Per risolvere questo problema, utilizzare il nodo `TSplineSurface.CompressIndices`.

Nell'esempio seguente, `TSplineTopology.DecomposedEdges` viene utilizzato per recuperare i bordi di una superficie T-Spline e un nodo `TSplineEdge.Index` viene quindi utilizzato per ottenere gli indici dei bordi forniti.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
