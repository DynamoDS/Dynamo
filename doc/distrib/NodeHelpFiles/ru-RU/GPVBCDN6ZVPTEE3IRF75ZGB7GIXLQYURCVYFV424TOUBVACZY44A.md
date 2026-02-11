<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces --->
<!--- GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A --->
## Подробности
Узел `TSplineSurface.BridgeEdgesToFaces` соединяет набор ребер с набором граней, относящихся либо к одной и той же поверхности, либо к двум разным поверхностям. Количество ребер, образующих грани, должно быть одинаковым или кратным количеству ребер с другой стороны перемычки. Узел требует задания входных параметров, перечисленных ниже. Первых трех входных параметров достаточно для создания перемычки; остальные входные параметры являются необязательными. Итоговая поверхность представляет собой дочерний объект поверхности, к которой относится первая группа ребер.

- `TSplineSurface`: the surface to bridge
— `firstGroup`: ребра из выбранной поверхности TSplineSurface.
— `secondGroup`: грани либо с выбранной Т-сплайновой поверхности, либо с другой поверхности.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


В приведенном ниже примере создаются две Т-сплайновые плоскости; узлы `TSplineTopology.VertexByIndex` и `TSplineTopology.FaceByIndex` извлекают наборы ребер и граней. Для создания перемычки грани и ребра вместе с одной из поверхностей используются в качестве входных данных для узла `TSplineSurface.BrideEdgesToFaces`. К созданной перемычке добавляются пролеты путем изменения входного параметра `spansCounts`. Если в качестве значения входного параметра `followCurves` используется кривая, перемычка проходит в направлении, заданном этой кривой. Входные параметры `keepSubdCreases`, `frameRotations`, `firstAlignVertices` и `secondAlignVertices` демонстрируют возможности настройки формы перемычки.

## Файл примера

![Example](./GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A_img.gif)

