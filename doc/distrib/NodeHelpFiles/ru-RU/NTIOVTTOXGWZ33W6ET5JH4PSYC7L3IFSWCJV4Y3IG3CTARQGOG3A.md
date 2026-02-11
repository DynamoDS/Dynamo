<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## Подробности
Узел `TSplineSurface.BridgeEdgesToEdges` соединяет два набора ребер, относящихся либо к одной и той же поверхности, либо к двум разным поверхностям. Узел требует задания входных параметров, перечисленных ниже. Первых трех входных параметров достаточно для создания перемычки; остальные входные параметры являются необязательными. Итоговая поверхность представляет собой дочерний объект поверхности, к которой относится первая группа ребер.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
— `secondGroup`: ребра либо с выбранной Т-сплайновой поверхности, либо с другой поверхности. Количество ребер должно быть равно количеству ребер с другой стороны перемычки или кратным ему.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


В приведенном ниже примере создаются две Т-сплайновые плоскости, после чего грань в центре каждой из них удаляется с помощью узла `TSplineSurface.DeleteEdges`. Узел `TSplineTopology.VertexByIndex` извлекает ребра вокруг удаленной грани. Для создания перемычки две группы ребер вместе с одной из поверхностей используются в качестве входных данных для узла `TSplineSurface.BrideEdgesToEdges`. К созданной перемычке добавляются пролеты путем изменения входного параметра `spansCounts`. Если в качестве значения входного параметра `followCurves` используется кривая, перемычка проходит в направлении, заданном этой кривой. Входные параметры `keepSubdCreases`, `frameRotations`, `firstAlignVertices` и `secondAlignVertices` демонстрируют возможности настройки формы перемычки.

## Файл примера

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

