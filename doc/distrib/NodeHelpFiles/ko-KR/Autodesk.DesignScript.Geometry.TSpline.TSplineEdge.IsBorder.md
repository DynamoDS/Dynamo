## In-Depth
`TSplineEdge.IsBorder`는 입력 T-Spline 모서리가 경계인 경우 `True`를 반환합니다.

아래 예에서는 두 T-Spline 표면의 모서리를 조사합니다. 표면은 원통과 두꺼운 원통 버전입니다. 모든 모서리를 선택하려면 두 경우 모두 정수 범위가 0~n인 색인 입력과 함께 `TSplineTopology.EdgeByIndex` 노드가 사용됩니다. 여기서 n은 `TSplineTopology.EdgesCount`에서 제공되는 모서리 수입니다. 이는 `TSplineTopology.DecomposedEdges`를 사용하여 모서리를 직접 선택하는 방법의 대안입니다. 두꺼운 원통의 경우 모서리 색인의 순서를 변경하기 위해 `TSplineSurface.CompressIndices`가 추가로 사용됩니다.
`TSplineEdge.IsBorder` 노드는 어느 모서리가 경계 모서리인지 확인하는 데 사용됩니다. `TSplineEdge.UVNFrame` 및 `TSplineUVNFrame.Position` 노드를 통해 평면 원통의 경계 모서리 위치가 강조 표시됩니다. 두꺼운 원통에는 경계 모서리가 없습니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
