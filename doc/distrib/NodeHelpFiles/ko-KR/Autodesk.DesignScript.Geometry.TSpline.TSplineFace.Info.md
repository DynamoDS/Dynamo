## In-Depth
`TSplineFace.Info`는 T-Spline 면의 다음 특성을 반환합니다.
- `uvnFrame`: 헐의 점, T-Spline 면의 U 벡터, V 벡터 및 법선 벡터
- `index`: 면의 색인
- `valence`: 면을 형성하는 정점 또는 모서리의 수
- `sides`: 각 T-Spline 면의 모서리 수

아래 예에서는 `TSplineSurface.ByBoxCorners` 및 `TSplineTopology.RegularFaces`가 각각 T-Spline을 작성하고 해당 면을 선택하는 데 사용됩니다. `List.GetItemAtIndex`는 T-Spline의 특정 면을 선택하는 데 사용되고 `TSplineFace.Info`는 해당 특성을 찾는 데 사용됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
