<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## Подробности
Узел `TSplineSurface.BridgeEdgesToFaces` соединяет два набора граней, относящихся либо к одной и той же поверхности, либо к двум разным поверхностям. Узел требует задания входных параметров, перечисленных ниже. Первых трех входных параметров достаточно для создания перемычки; остальные входные параметры являются необязательными. Итоговая поверхность представляет собой дочерний объект поверхности, к которой относится первая группа ребер.

В приведенном ниже примере создается поверхность тора с помощью узла `TSplineSurface.ByTorusCenterRadii`. Две ее грани выбираются для использования вместе с самой поверхностью тора в качестве входных параметров узла `TSplineSurface.BridgeFacesToFaces`. Остальные входные параметры иллюстрируют возможности по дальнейшей настройке перемычки.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
— `cleanBorderBridges`: (необязательно) удаляет перемычки между перемычками границ для предотвращения сгибов.
— `keepSubdCreases`: (необязательно) сохраняет сгибы SubD входной топологии, что приводит к образованию сгибов в начале и конце перемычки. Поверхность тора не имеет согнутых ребер, поэтому эти входные параметры не влияют на форму.
— `firstAlignVertices` (необязательно) и `secondAlignVertices`: перемычка слегка поворачивается в результате задания смещенной пары вершин.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## Файл примера

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
