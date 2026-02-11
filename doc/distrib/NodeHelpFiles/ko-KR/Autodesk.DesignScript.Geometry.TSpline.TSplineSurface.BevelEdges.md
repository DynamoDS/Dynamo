## In-Depth
`TSplineSurface.BevelEdges` 노드는 선택한 모서리 또는 모서리 그룹을 면을 따라 양방향으로 간격띄우기하고 원래 모서리를 채널을 형성하는 일련의 모서리로 대체합니다.

아래 예에서는 T-Spline 상자 원형의 모서리 그룹이 `TSplineSurface.BevelEdges` 노드의 입력으로 사용됩니다. 이 예는 다음 입력이 결과에 미치는 영향을 보여줍니다.
- `percentage`는 인접하는 면을 따라 새로 작성된 모서리의 분포를 제어하며, 값이 0에 인접한 경우 새 모서리를 원래 모서리에 더 가깝게 배치하고 값이 1에 가까운 경우 더 멀리 배치합니다.
- `numberOfSegments`는 채널에 있는 새 면의 수를 제어합니다.
- `KeepOnFace`는 베벨 모서리가 원래 면의 평면에 배치되는지 여부를 정의합니다. 값이 True로 설정된 경우 둥글기 입력이 영향을 받지 않습니다.
- `roundness`는 베벨이 둥근 정도를 제어하고 0에서 1 사이의 값이어야 합니다. 0은 직선 베벨을, 1은 둥근 베벨을 반환합니다.

모양을 더 잘 이해하기 위해 상자 모드가 켜지는 경우가 있습니다.


## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
