## In-Depth
`TSplineVertex.Info`는 T-Spline 정점의 다음 특성을 반환합니다.
- `uvnFrame`: 헐의 점, U 벡터, V 벡터 및 T-Spline 정점의 법선 벡터
- `index`: T-Spline 표면에서 선택한 정점의 색인
- `isStarPoint`: 선택한 정점이 별 점인지 여부
- `IsTpoint`: 선택한 정점이 T 점인지 여부
- `isManifold`: 선택한 정점이 매니폴드인지 여부
- `valence`: 선택한 T-Spline 정점의 모서리 수
- `functionalValence`: 정점의 기능적 Valence입니다. 자세한 내용은 `TSplineVertex.FunctionalValence` 노드에 관한 문서를 참조하십시오.

아래 예에서는 `TSplineSurface.ByBoxCorners` 및 `TSplineTopology.VertexByIndex`가 T-Spline 표면을 작성하고 해당 정점을 선택하는 데 각각 사용됩니다. `TSplineVertex.Info`는 선택한 정점에 대한 위의 정보를 수집하는 데 사용됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
