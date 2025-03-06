## In-Depth
`TSplineEdge.IsBorder` devuelve `True` (verdadero) si la arista de T-Spline de entrada es un borde.

En el ejemplo siguiente, se investigan las aristas de dos superficies de T-Spline. Las superficies son un cilindro y su versión engrosada. Para seleccionar todas las aristas, se utilizan en ambos casos los nodos `TSplineTopology.EdgeByIndex` con la entrada de los índices (un rango de enteros que va de 0 a n, donde n es el número de aristas proporcionado por `TSplineTopology.EdgesCount`). Se trata de una alternativa a la selección directa de aristas mediante `TSplineTopology.DecomposedEdges`. En el caso de un cilindro engrosado se utiliza además `TSplineSurface.CompressIndices` para reordenar los índices de las aristas.
Se utiliza un nodo `TSplineEdge.IsBorder` para comprobar cuáles de las aristas son aristas de borde. La posición de las aristas de borde del cilindro plano se resaltan con ayuda de los nodos `TSplineEdge.UVNFrame` y `TSplineUVNFrame.Position`. El cilindro engrosado no tiene aristas de borde.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
