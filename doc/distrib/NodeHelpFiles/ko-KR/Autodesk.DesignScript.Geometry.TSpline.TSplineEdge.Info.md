## In-Depth
`TSplineEdge.Info`는 T-Spline 표면 모서리의 다음 특성을 반환합니다.
- `uvnFrame`: 헐의 점, U 벡터, V 벡터 및 T-Spline 모서리의 법선 벡터
- `index`: 모서리의 색인
- `isBorder`: 선택한 모서리가 T-Spline 표면의 경계인지 여부
- `IsManifold`: 선택한 모서리가 매니폴드인지 여부

아래 예에서는 `TSplineTopology.DecomposedEdges`가 T-Spline 원통 원형 표면의 모든 모서리 리스트를 가져오는 데 사용되고 `TSplineEdge.Info`는 해당 특성을 조사하는 데 사용됩니다.


## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
