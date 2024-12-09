<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## 상세
아래 예에서는 돌출, 세분화 및 당겨진 정점 및 면이 있는 평면형 T-Spline 표면이 `TSplineTopology.DecomposedVertices` 노드로 검사되며, 여기서는 T-Spline 표면에 포함된 다음 정점 유형 리스트를 반환합니다.

- `all`: 모든 정점 리스트
- `regular`: 일반 정점 리스트
- `tPoints`: T 점 정점 리스트
- `starPoints`: 별 점 정점 리스트
- `nonManifold`: 비 매니폴드 정점 리스트
- `border`: 경계 정점 리스트
- `inner`: 내부 정점 리스트

`TSplineVertex.UVNFrame` 및 `TSplineUVNFrame.Position` 노드는 표면의 여러 정점 유형을 강조 표시하는 데 사용됩니다.

___
## 예제 파일

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
