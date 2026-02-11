## In-Depth
`TSplineSurface.CreaseEdges`는 T-Spline 표면의 지정된 모서리에 날카로운 각진 부분을 추가합니다.
아래 예에서는 T-Spline 표면이 T-Spline 원환에서 생성됩니다. 모서리는 `TSplineTopology.EdgeByIndex` 노드를 사용하여 선택되고 각진 부분은 `TSplineSurface.CreaseEdges` 노드를 통해 해당 모서리에 적용됩니다. 모서리의 두 모서리에 있는 정점도 각이 집니다. 선택한 모서리의 위치는 `TSplineEdge.UVNFrame` 및 `TSplineUVNFrame.Provision` 노드를 통해 미리 볼 수 있습니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
