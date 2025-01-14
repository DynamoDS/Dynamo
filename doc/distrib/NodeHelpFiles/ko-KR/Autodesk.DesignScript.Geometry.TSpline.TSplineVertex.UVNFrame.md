## In-Depth
이 노드는 정점 위치 및 방향을 시각화하고 U, V 또는 N 벡터를 사용하여 T-Spline 표면을 추가로 조작하는 데 유용할 수 있는 TSplineUVNFrame 객체를 반환합니다.

아래 예에서는 `TSplineVertex.UVNFrame` 노드가 선택한 정점의 UVN 프레임을 가져오는 데 사용됩니다. 그런 다음 UVN 프레임이 정점의 법선을 반환하는 데 사용됩니다. 마지막으로 법선 방향이 `TSplineSurface.MoveVertices` 노드를 사용하여 정점을 이동하는 데 사용됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)
