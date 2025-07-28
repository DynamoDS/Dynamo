## In-Depth
면의 UVNFrame은 법선 벡터 및 UV 방향을 반환하여 면 위치 및 방향에 대한 유용한 정보를 제공합니다.
아래 예에서는 `TSplineFace.UVNFrame` 노드가 쿼드볼 원형에 있는 면의 분포를 시각화하는 데 사용됩니다. `TSplineTopology.DecomposedFaces` 노드가 모든 면을 쿼리하는 데 사용된 후 `TSplineFace.UVNFrame` 노드가 면 중심의 위치를 점으로 검색하는 데 사용됩니다. 점은 `TSplineUVNFrame.Position` 노드를 사용하여 시각화됩니다. 레이블은 노드의 마우스 오른쪽 버튼 클릭 메뉴에서 레이블 표시를 활성화하면 배경 미리보기에 표시됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)
