<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## Подробности
В приведенном ниже примере плоская Т-сплайновая поверхность с выдавленными, разделенными и вытянутыми вершинами и гранями проверяется с помощью узла `TSplineTopology.DecomposedVertices`, который возвращает список следующих типов вершин, содержащихся в T-сплайновой поверхности:

— `all`: список всех вершин;
— `regular`: список обычных вершин;
— `tPoints`: список Т-точечных вершин;
— `starPoints`: список вершин в нулевых точках;
— `nonManifold`: список неоднородных вершин;
— `border`: список вершин границ;
— `inner`: список внутренних вершин.

Узлы `TSplineVertex.UVNFrame` и `TSplineUVNFrame.Position` используются для выделения различных типов вершин поверхности.

___
## Файл примера

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
