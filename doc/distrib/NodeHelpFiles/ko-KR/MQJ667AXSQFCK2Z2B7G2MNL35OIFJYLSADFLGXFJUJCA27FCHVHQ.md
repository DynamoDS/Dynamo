<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## 상세
`TSplineSurface.BridgeEdgesToFaces`는 동일한 표면 또는 두 개의 다른 표면의 두 개의 면 세트를 연결합니다. 노드에는 아래에 설명된 입력이 필요합니다. 교량을 생성하기 위해서는 처음 3개의 입력이면 충분하고 나머지 입력은 선택 사항입니다. 결과 표면은 첫 번째 모서리 그룹이 속한 표면의 하위 항목입니다.

아래 예에서는 원환 표면이 `TSplineSurface.ByTorusCenterRadii`를 사용하여 작성됩니다. 두 개의 면이 선택되고 원환 표면과 함께 `TSplineSurface.BridgeFacesToFaces` 노드의 입력으로 사용됩니다. 나머지 입력은 교량을 추가로 조정할 수 있는 방법을 보여줍니다.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`: (선택 사항) 각진 부분을 방지하기 위해 경계 교량 사이 교량을 삭제합니다.
- `keepSubdCreases`: (선택 사항) 입력 토폴로지의 하위 각진 부분을 유지하여 교량의 시작 및 끝을 각지게 처리합니다. 원환 표면에는 각진 모서리가 없으므로 이 입력은 모양에 영향을 주지 않습니다.
- `firstAlignVertices`(선택 사항) 및 `secondAlignVertices`: 이동된 정점 쌍을 지정하면 교량이 라이트 회전을 획득합니다.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## 예제 파일

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
