## In-Depth
Узел `TSplineSurface.CreaseEdges` добавляет острый сгиб к указанному ребру на Т-сплайновой поверхности.
В приведенном ниже примере Т-сплайновая поверхность создается на основе Т-сплайнового тора. Ребро выбирается с помощью узла `TSplineTopology.EdgeByIndex`, а сгиб применяется к этому ребру с помощью узла `TSplineSurface.CreaseEdges`. Сгибы также применяются к вершинам на обоих ребрах. Положение выбранного ребра можно просмотреть с помощью узлов `TSplineEdge.UVNFrame` и `TSplineUVNFrame.Poision`.

## Файл примера

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
