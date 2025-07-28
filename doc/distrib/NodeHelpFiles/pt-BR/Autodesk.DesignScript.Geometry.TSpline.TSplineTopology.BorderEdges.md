## Em profundidade
`TSplineTopology.BorderEdges` retorna uma lista de arestas de borda contidas na superfície da T-Spline.

No exemplo abaixo, são criadas duas superfícies T-Spline por meio de `TSplineSurface.ByCylinderPointsRadius`; uma é uma superfície aberta enquanto a outra é engrossada usando `TSplineSurface.Thicken`, que a transforma em uma superfície fechada. Quando ambas são examinadas com o nó `TSplineTopology.BorderEdges`, a primeira retorna uma lista de arestas de borda enquanto a segunda retorna uma lista vazia. Isso é porque como a superfície está fechada, não há arestas de borda.
___
## Arquivo de exemplo

![TSplineTopology.BorderEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges_img.jpg)
