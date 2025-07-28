## 상세
아래 예에서는 T-Spline 표면이 NURBS 곡선을 돌출시켜 작성됩니다. `TSplineTopology.EdgeByIndex` 노드를 이용해 모양 양쪽에 3개씩 모서리 6개가 선택됩니다. 표면과 함께 2개의 모서리 세트가 `TSplineSurface.MergeEdges` 노드로 전달됩니다. 모서리 그룹의 순서는 모양에 영향을 주며, 모서리의 첫 번째 그룹이 동일한 위치에 있는 두 번째 그룹과 만나도록 변위됩니다. `insertCreases` 입력은 병합된 모서리를 따라 이음새를 각지게 만드는 옵션을 추가합니다. 병합 작업의 결과는 더 나은 미리 보기를 위해 측면으로 이동됩니다.
___
## 예제 파일

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
