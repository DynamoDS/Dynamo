## In-Depth
`TSplineSurface.CreaseEdges` 노드와 반대로 이 노드는 T-Spline 표면에서 지정된 모서리의 각진 부분을 제거합니다.
아래 예에서는 T-Spline 표면이 T-Spline 원환에서 생성됩니다. 모든 모서리는 `TSplineTopology.EdgeByIndex` 및 `TSplineTopology.EdgesCount` 노드를 사용하여 선택되고, 각진 부분은 `TSplineSurface.CreaseEdges` 노드를 통해 모든 모서리에 적용됩니다. 그런 다음 색인이 0~7인 모서리 하위 세트가 선택되고 이번에는 `TSplineSurface.UncreaseEdges` 노드를 사용하여 반전 작업이 적용됩니다. 선택한 모서리의 위치는 `TSplineEdge.UVNFrame` 및 `TSplineUVNFrame.Poision` 노드를 통해 미리 볼 수 있습니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
