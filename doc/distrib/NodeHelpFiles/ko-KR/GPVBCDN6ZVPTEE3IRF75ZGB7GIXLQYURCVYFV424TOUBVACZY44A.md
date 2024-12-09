<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces --->
<!--- GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A --->
## 상세
`TSplineSurface.BridgeEdgesToFaces`는 모서리 세트를 동일한 표면 또는 두 개의 다른 표면의 면 세트와 연결합니다. 면을 구성하는 모서리는 개수가 일치하거나 교량 반대쪽의 모서리 수의 배수여야 합니다. 노드에는 아래에 설명된 입력이 필요합니다. 교량을 생성하기 위해서는 처음 3개의 입력이면 충분하고 나머지 입력은 선택 사항입니다. 결과 표면은 첫 번째 모서리 그룹이 속한 표면의 하위 항목입니다.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: 선택한 TSplineSurface의 모서리
- `secondGroup`: 선택한 동일한 T-Spline 표면의 면 또는 다른 표면의 면.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


아래 예에서는 두 개의 T-Spline 평면이 작성되고 모서리 및 면 세트가 `TSplineTopology.VertexByIndex` 및 `TSplineTopology.FaceByIndex` 노드를 사용하여 수집됩니다. 교량을 작성하기 위해 면 및 모서리가 표면 중 하나와 함께 `TSplineSurface.BrideEdgesToFaces` 노드의 입력으로 사용됩니다. 이를 통해 교량이 작성됩니다. `SpansCounts` 입력을 편집하면 교량에 더 많은 스팬이 추가됩니다. 곡선이 `followCurves`의 입력으로 사용되면 교량은 제공된 곡선의 방향을 따릅니다. `keepSubdCreases`,`frameRotations`, `firstAlignVertices` 및 `secondAlignVertices` 입력은 교량의 모양을 미세 조정할 수 있는 방법을 보여줍니다.

## 예제 파일

![Example](./GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A_img.gif)

