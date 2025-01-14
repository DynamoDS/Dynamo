<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedEdges --->
<!--- 7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ --->
## Подробности
В приведенном ниже примере плоская Т-сплайновая поверхность с выдавленными, разделенными и вытянутыми вершинами и гранями проверяется с помощью узла `TSplineTopology.DecomposedEdges`, который возвращает список следующих типов ребер, содержащихся в Т-сплайновой поверхности:

— `all`: список всех ребер;
— `nonManifold`: список неоднородных ребер;
— `border`: список ребер границ;
— `inner`: список внутренних ребер.


Узел `Edge.CurveGeometry` используется для выделения различных типов ребер поверхности.
___
## Файл примера

![TSplineTopology.DecomposedEdges](./7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ_img.gif)
