## In-Depth
`TSplineVertex.IsStarPoint`는 정점이 별 점인지 여부를 반환합니다.

별 점은 3개, 5개 또는 그 이상의 모서리가 합쳐질 때 존재합니다. 별 점은 기본적으로 상자 또는 쿼드볼 원형에서 나타나며 T-Spline 면을 돌출시키거나 면을 삭제하거나 병합을 수행할 때 가장 일반적으로 작성됩니다. 일반 및 T 점 정점과 달리 별 점은 제어점의 직사각형 행으로 제어되지 않습니다. 별 점은 주변 영역을 제어하기 더 어렵게 만들고 왜곡을 작성할 수 있으므로 필요한 경우에만 사용해야 합니다. 별 점 배치가 잘못되면 모델에 더 각진 부분(예: 각진 모서리, 곡률이 크게 변하는 부분 또는 열린 표면의 모서리)이 나타납니다.

별 점은 T-Spline이 경계 표현(BREP)으로 변환되는 방법도 결정합니다. T-Spline이 BREP로 변환되면 각 별 점에서 별도의 표면으로 분할됩니다.

아래 예에서는 `TSplineVertex.IsStarPoint`가 `TSplineTopology.VertexByIndex`로 선택된 정점이 별 점인지 여부를 쿼리하는 데 사용됩니다.


## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
