<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices --->
<!--- HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ --->
## Em profundidade
`TSplineTopology.BorderVertices` retorna uma lista de vértices de borda contidos em uma superfície da T-Spline.

No exemplo abaixo, são criadas duas superfícies T-Spline por meio de `TSplineSurface.ByCylinderPointsRadius`. Uma é uma superfície aberta enquanto a outra é engrossada usando `TSplineSurface.Thicken`, que a transforma em uma superfície fechada. Quando ambas são examinadas com o nó `TSplineTopology.BorderVertices`, a primeira retorna uma lista de vértices de borda enquanto a segunda retorna uma lista vazia. Isso porque como a superfície está fechada, não há vértices de borda.
___
## Arquivo de exemplo

![TSplineTopology.BorderVertices](./HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ_img.jpg)
