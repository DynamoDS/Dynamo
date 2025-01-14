## In-Depth
Обратите внимание, что в топологии Т-сплайновой поверхности индексы `Face`, `Edge` и `Vertex` не всегда совпадают с порядковым номером элемента в списке. Для решения этой проблемы используйте узел `TSplineSurface.CompressIndices`.

В приведенном ниже примере узел `TSplineTopology.DecomposedEdges` используется для извлечения ребер границ Т-сплайновой поверхности, после чего узел `TSplineEdge.Index` используется для получения индексов предоставленных ребер.

## Файл примера

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
