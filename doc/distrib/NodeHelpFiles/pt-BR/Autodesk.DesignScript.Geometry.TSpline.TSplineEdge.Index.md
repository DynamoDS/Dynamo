## In-Depth
Observe que em uma topologia de superfície da T-Spline, os índices de `Face`, `Edge` e `Vertex` não coincidem necessariamente com o número de sequência do item na lista. Use o nó `TSplineSurface.CompressIndices` para solucionar esse problema.

No exemplo abaixo, `TSplineTopology.DecomposedEdges` é usado para recuperar as arestas de borda de uma superfície da T-Spline e um nó `TSplineEdge.Index` é usado para obter os índices das arestas fornecidas.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
