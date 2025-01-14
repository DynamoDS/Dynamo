## In-Depth
В отличие от узла `TSplineSurface.CreaseEdges` этот узел удаляет сгиб заданного ребра на Т-сплайновой поверхности.
В приведенном ниже примере Т-сплайновая поверхность создается на основе Т-сплайнового тора. Все ребра выбираются с помощью узлов `TSplineTopology.EdgeByIndex` и `TSplineTopology.EdgesCount`. Сгиб применяется ко всем ребрам с помощью узла `TSplineSurface.CreaseEdges`. Затем выбирается набор ребер с индексами от 0 до 7, к которым применяется обратная операция — на этот раз с помощью узла `TSplineSurface.UncreaseEdges`. Для предварительного просмотра выбранных ребер используются узлы `TSplineEdge.UVNFrame` и `TSplineUVNFrame.Poision`.

## Файл примера

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
