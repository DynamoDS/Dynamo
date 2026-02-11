## Em profundidade
No exemplo abaixo, todos os vértices internos de uma superfície de plano da T-Spline são coletados usando o nó `TSplineTopology.InnerVertices`. Os vértices, junto com a superfície à qual pertencem, são usados como entrada para o nó `TSplineSurface.PullVertices`. A entrada `geometry` é uma esfera localizada acima da superfície do plano. A entrada `surfacePoints` é definida como False e os pontos de controle são usados para executar a operação de extração.
___
## Arquivo de exemplo

![TSplineSurface.PullVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.PullVertices_img.jpg)
