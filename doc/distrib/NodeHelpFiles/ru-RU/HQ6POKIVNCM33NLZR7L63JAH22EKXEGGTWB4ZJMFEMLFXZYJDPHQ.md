<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices --->
<!--- HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ --->
## Подробности
Узел `TSplineTopology.BorderVertices` возвращает список вершин границ, содержащихся в Т-сплайновой поверхности.

В приведенном ниже примере создаются две Т-сплайновые поверхности с помощью узла `TSplineSurface.ByCylinderPointsRadius`. Одна из поверхностей является незамкнутой, другая — утолщается с помощью узла `TSplineSurface.Thicken`, в результате чего становится замкнутой. Обе поверхности проверяются с помощью узла `TSplineTopology.BorderVertices`, после чего для первой из них возвращается список вершин границ, а для второй — пустой список. Это связано с тем, что у второй поверхности нет вершин границ, так как она является замкнутой.
___
## Файл примера

![TSplineTopology.BorderVertices](./HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ_img.jpg)
