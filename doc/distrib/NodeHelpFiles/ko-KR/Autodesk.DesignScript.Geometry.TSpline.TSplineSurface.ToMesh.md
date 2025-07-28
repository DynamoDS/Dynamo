## 상세
아래 예에서는 간단한 T-Spline 상자 표면이 `TSplineSurface.ToMesh` 노드를 사용하여 메쉬로 변환됩니다. `minSegments` 입력은 각 방향의 면에 대한 최소 세그먼트 수를 정의하며, 메쉬 정의를 제어하는 데 중요합니다. `tolerance` 입력은 지정된 공차 내에서 원래 표면과 일치하도록 더 많은 정점 위치를 추가하여 부정확성을 수정합니다. 결과적으로 `Mesh.VertexPositions` 노드를 사용하여 해당 정의를 미리 볼 수 있는 메쉬가 생성됩니다.
출력 메쉬에는 MeshToolkit 노드를 사용하는 경우 유의해야 하는 삼각형 및 사각형이 모두 포함될 수 있습니다.
___
## 예제 파일

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
