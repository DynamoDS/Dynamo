## In-Depth
`TSplineVertex.Index` retorna o número de índice do vértice escolhido na superfície da T-Spline. Observe que em uma topologia de superfície da T-Spline, os índices de Face, Edge e Vertex não coincidem necessariamente com o número de sequência do item na lista. Use o nó `TSplineSurface.CompressIndices` para resolver esse problema.

No exemplo abaixo, `TSplineTopology.StarPointVertices` é usado em um primitivo da T-Spline na forma de uma caixa. `TSplineVertex.Index` é usado para consultar os índices de vértices de ponto de estrela e `TSplineTopology.VertexByIndex` retorna os vértices selecionados para edição posterior.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
