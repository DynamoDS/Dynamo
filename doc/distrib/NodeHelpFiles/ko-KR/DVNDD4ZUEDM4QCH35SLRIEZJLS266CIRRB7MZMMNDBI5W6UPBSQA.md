<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## 상세
`TSplineSurface.BridgeToFacesToEdges`는 모서리 세트를 동일한 표면 또는 두 개의 다른 표면의 면 세트와 연결합니다. 면을 구성하는 모서리는 개수가 일치하거나 교량 반대쪽의 모서리 수의 배수여야 합니다. 노드에는 아래에 설명된 입력이 필요합니다. 교량을 생성하기 위해서는 처음 3개의 입력이면 충분하고 나머지 입력은 선택 사항입니다. 결과 표면은 첫 번째 모서리 그룹이 속한 표면의 하위 항목입니다.

- `TSplineSurface`: 교량을 놓을 표면
- `firstGroup`: 선택한 TSplineSurface의 면
- `secondGroup`: 선택한 동일한 T-Spline 표면 또는 다른 표면의 모서리. 모서리 수는 개수가 일치하거나 교량 반대쪽의 모서리 수의 배수여야 합니다.
- `followCurves`: (선택 사항) 교량이 따라가는 곡선. 이 입력이 없을 경우 교량은 직선을 따릅니다
- `frameRotations`: (선택 사항) 선택한 모서리를 연결하는 교량 돌출의 회전 수.
- `spanCounts`: (선택 사항) 선택한 모서리를 연결하는 교량 돌출의 스팬/세그먼트 수. 스팬 수가 너무 적으면 수가 늘어날 때까지 특정 옵션을 사용할 수 없습니다.
- `cleanBorderBridges`: (선택 사항) 각진 부분을 방지하기 위해 경계 교량 간 교량을 삭제합니다
- `keepSubdCreases`: (선택 사항) 입력 토폴로지의 하위 각진 부분을 유지하여 교량의 시작과 끝을 각지게 처리합니다.
- `firstAlignVertices`(선택 사항) 및 `secondAlignVertices`: 가장 가까운 정점 쌍을 연결하도록 자동으로 선택하는 대신 두 정점 세트 간에 정렬을 적용합니다.
- `flipAlignFlags`: (선택 사항) 정렬할 정점의 방향을 반전합니다.


아래 예에서는 두 개의 T-Spline 평면이 작성되고 모서리 및 면 세트가 `TSplineTopology.VertexByIndex` 및 `TSplineTopology.FaceByIndex` 노드를 사용하여 수집됩니다. 교량을 작성하기 위해 면 및 모서리가 표면 중 하나와 함께 `TSplineSurface.BrideFacesToEdges` 노드의 입력으로 사용됩니다. 이를 통해 교량이 작성됩니다. `spansCounts` 입력을 편집하면 교량에 더 많은 스팬이 추가됩니다. 곡선이 'followCurves'의 입력으로 사용되면 교량은 제공된 곡선의 방향을 따릅니다. `keepSubdCreases`,`frameRotations`, `firstAlignVertices` 및 `secondAlignVertices` 입력은 교량의 모양을 미세 조정할 수 있는 방법을 보여줍니다.

## 예제 파일

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
