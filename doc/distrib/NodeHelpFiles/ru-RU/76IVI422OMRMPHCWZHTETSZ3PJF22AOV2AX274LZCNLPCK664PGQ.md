<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DeleteVertices --->
<!--- 76IVI422OMRMPHCWZHTETSZ3PJF22AOV2AX274LZCNLPCK664PGQ --->
## Подробности
В приведенном ниже примере создается плоская Т-сплайновая поверхность-примитив с помощью узла `TSplineSurface.ByPlaneOriginNormal`. Набор вершин выбирается с помощью узла `TSplineTopology.VertexByIndex` и передается в качестве входного параметра в узел `TSplineSurface.DeleteVertices`. В результате все ребра, соединяющиеся в выбранной вершине, также удаляются.

## Файл примера

![Example](./76IVI422OMRMPHCWZHTETSZ3PJF22AOV2AX274LZCNLPCK664PGQ_img.jpg)
