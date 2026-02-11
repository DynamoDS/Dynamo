## In-Depth
Узел `TSplineVertex.IsTPoint` определяет, является ли вершина Т-точкой. Т-точки — это вершины в конце частичных рядов управляющих точек.

В приведенном ниже примере узел `TSplineSurface.SubdivideFaces` используется для Т-сплайновой рамки-примитива с целью продемонстрировать один из нескольких способов добавления Т-точек на поверхность. Узел `TSplineVertex.IsTPoint` проверяет, является ли вершина с заданным индексом T-точкой. Для более эффективной визуализации положения Т-точек используются узлы `TSplineVertex.UVNFrame` и `TSplineUVNFrame.Position`.



## Файл примера

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
