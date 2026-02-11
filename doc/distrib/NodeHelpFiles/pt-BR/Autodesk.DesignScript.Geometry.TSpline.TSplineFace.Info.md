## In-Depth
`TSplineFace.Info` retorna as seguintes propriedades de uma face da T-Spline:
- `uvnFrame`: ponto na cobertura, vetor U, vetor V e vetor normal da face da T-Spline
- `index`: o índice da face
- `valence`: número de vértices ou arestas que formam uma face
- `sides`: o número de arestas de cada face da T-Spline

No exemplo abaixo, `TSplineSurface.ByBoxCorners` e `TSplineTopology.RegularFaces` são usados para criar respectivamente uma T-Spline e selecionar suas faces. `List.GetItemAtIndex` é usado para selecionar uma face específica da T-Spline e `TSplineFace.Info` é usado para descobrir suas propriedades.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
