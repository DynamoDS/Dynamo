## Подробности
В приведенном ниже примере создается Т-сплайновая рамка с помощью узла `TSplineSurface.ByBoxLengths` с заданными началом координат, шириной, длиной, высотой, пролетами и симметрией.
Затем узел `EdgeByIndex` используется для выбора ребра из списка ребер для созданной поверхности. Выбранное ребро, а также его симметричные аналоги перемещаются вдоль соседних ребер с помощью узла `TSplineSurface.SlideEdges`.
___
## Файл примера

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
