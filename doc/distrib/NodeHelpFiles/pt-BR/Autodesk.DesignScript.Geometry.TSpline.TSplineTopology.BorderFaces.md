## Em profundidade
`TSplineTopology.BorderFaces` retorna a lista de faces de borda contidas na superfície da T-Spline.

No exemplo abaixo, são criadas duas superfícies da T-Spline por meio de `TSplineSurface.ByCylinderPointsRadius`. Uma é uma superfície aberta, enquanto a outra é engrossada usando `TSplineSurface.Thicken`, que a transforma em uma superfície fechada. Quando ambas são examinadas com o nó `TSplineTopology.BorderFaces`, a primeira retorna uma lista de faces de bordas enquanto a segunda retorna uma lista vazia. Isto acontece porque, como a superfície está fechada, não existem faces de bordas.
___
## Arquivo de exemplo

![TSplineTopology.BorderFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderFaces_img.jpg)
