## In-Depth
`TSplineVertex.IsTPoint`는 정점이 T 점인지 여부를 반환합니다. T 점은 제어점의 부분 행 끝에 있는 정점입니다.

아래 예에서는 표면에 T 점을 추가하는 여러 방법 중 하나를 예로 보여주기 위해 `TSplineSurface.SubdivideFaces`가 T-Spline 상자 원형에 사용됩니다. `TSplineVertex.IsTPoint` 노드는 색인에 있는 정점이 T 점임을 확인하는 데 사용됩니다. T 점의 위치를 더 효과적으로 시각화하기 위해 `TSplineVertex.UVNFrame` 및 `TSplineUVNFrame.Position` 노드가 사용됩니다.



## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
