## In-Depth
`TSplineVertex.Info` retorna as seguintes propriedades de um vértice da T-Spline:
- `uvnFrame`: ponto na cobertura, vetor U, vetor V e vetor normal do vértice da T-Spline
- `index`: o índice do vértice escolhido na superfície da T-Spline
- `isStarPoint`: se o vértice escolhido é um ponto de estrela
- `isTpoint`: se o vértice escolhido é um ponto T
- `isManifold`: se o vértice escolhido é múltiplo
- `valence`: número de arestas no vértice da T-Spline escolhido
- `functionalValence`: a validade funcional de um vértice. Consulte a documentação do nó `TSplineVertex.FunctionalValence` para obter mais informações.

No exemplo abaixo, `TSplineSurface.ByBoxCorners` e `TSplineTopology.VertexByIndex` são usados para criar, respectivamente, uma superfície da T-Spline e selecionar seus vértices. `TSplineVertex.Info` é usado para coletar as informações acima sobre um vértice escolhido.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
