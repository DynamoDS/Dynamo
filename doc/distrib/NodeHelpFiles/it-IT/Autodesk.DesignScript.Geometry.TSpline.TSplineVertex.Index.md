## In-Depth
`TSplineVertex.Index` restituisce il numero di indice del vertice scelto sulla superficie T-Spline. Tenere presente che nella topologia di una superficie T-Spline, gli indici di Face, Edge e Vertex non coincidono necessariamente con il numero di sequenza dell'elemento nell'elenco. Per risolvere il problema, utilizzare il nodo `TSplineSurface.CompressIndices`.

Nell'esempio seguente, `TSplineTopology.StarPointVertices` viene utilizzato su una primitiva T-Spline a forma di parallelepipedo. `TSplineVertex.Index` viene quindi utilizzato per eseguire query sugli indici dei vertici con punti a stella e `TSplineTopology.VertexByIndex` restituisce i vertici selezionati per ulteriori modifiche.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
