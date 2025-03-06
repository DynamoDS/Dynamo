## Подробности
В приведенном ниже примере узел `TSplineTopology.VertexByIndex` извлекает вершину Т-сплайновой поверхности. Затем эта вершина используется в качестве входного значения для узла `TSplineSurface.MoveVertices`. Вершина перемещается в направлении, заданном входным параметром `vector`. Если для параметра `onSurface` задано значение `True`, перемещается поверхность; если `False` — управляющие точки.
___
## Файл примера

![TSplineSurface.MoveVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MoveVertices_img.jpg)
