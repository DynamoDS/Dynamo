<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
`TSplineSurface.UnweldEdges` 노드와 마찬가지로 이 노드는 정점 세트에서 용접 해제 작업을 수행합니다. 따라서 선택한 정점에서 결합하는 모든 모서리가 용접 해제됩니다. 연결을 유지하면서 정점 주위에 각진 변환을 작성하는 각진 부분 제거 작업과 달리 용접 해제는 불연속성을 작성합니다.

아래 예에서는 T-Spline 평면에서 선택한 정점 중 하나가 `TSplineSurface.UnweldVertices` 노드로 용접되지 않습니다. 선택한 정점을 둘러싼 모서리를 따라 불연속성이 적용되며, 이는 `TSplineSurface.MoveVertices` 노드를 사용하여 정점을 위로 당기면 표시됩니다.

## 예제 파일

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
