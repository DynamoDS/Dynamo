## In-Depth
Узел `TSplineVertex.Index` возвращает порядковый номер выбранной вершины на Т-сплайновой поверхности. Обратите внимание на то, что в топологии Т-сплайновой поверхности индексы Face, Edge и Vertex не всегда совпадают с порядковым номером элемента в списке. Для решения этой проблемы используйте узел `TSplineSurface.CompressIndices`.

В приведенном ниже примере узел `TSplineTopology.StarPointVertices` используется для Т-сплайнового примитива в форме рамки. Затем узел `TSplineVertex.Index` используется для запроса индексов вершин в нулевых точках, а узел `TSplineTopology.VertexByIndex` возвращает выбранные вершины для дальнейшего редактирования.

## Файл примера

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
