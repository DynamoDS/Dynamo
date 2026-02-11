## 상세
`TSplineSurface.FlattenVertices(vertices, parallelPlane)`는 입력으로 제공되는 `parallelPlane`과 정점을 정렬하여 지정된 정점 세트에 대한 제어점의 위치를 변경합니다.

아래 예에서는 T-Spline 평면 표면의 정점이 `TsplineTopology.VertexByIndex` 및 `TSplineSurface.MoveVertices` 노드를 사용하여 변위됩니다. 그런 다음 더 나은 미리 보기를 위해 표면이 측면으로 이동되고 `TSplineSurface.FlattenVertices(vertices, parallelPlane)` 노드의 입력으로 사용됩니다. 결과적으로 선택된 정점이 있는 새 표면이 제공된 평면에 평평하게 놓입니다.
___
## 예제 파일

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
