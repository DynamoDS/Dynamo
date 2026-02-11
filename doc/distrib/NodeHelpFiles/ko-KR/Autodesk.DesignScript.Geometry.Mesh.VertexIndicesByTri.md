## 상세
`Mesh.VertexIndicesByTri`는 각 메쉬 삼각형에 해당하는 정점 색인의 단순화된 리스트를 반환합니다. 색인은 3개씩 정렬되며 색인 그룹화는 `lengths` 입력이 3인 `List.Chop` 노드를 사용하여 쉽게 재구성할 수 있습니다.

아래 예제에서는 20개의 삼각형이 있는 `MeshToolkit.Mesh`가 `Geometry.Mesh`로 변환됩니다. `Mesh.VertexIndicesByTri`는 색인 리스트를 구하는 데 사용됩니다. 해당 리스트는 `List.Chop`을 사용하여 리스트를 3개씩 나누어집니다. `List.Transpose`를 사용하여 리스트 구조를 반전시켜 각 메쉬 삼각형의 A, B 및 C 점에 해당하는 20개 색인의 최상위 리스트 3개를 얻습니다. `IndexGroup.ByIndices` 노드는 각각 3개의 색인으로 구성된 색인 그룹을 작성하는 데 사용됩니다. 그런 다음 변환된 메쉬를 구하기 위해 `IndexGroups`의 구조적 리스트와 정점 리스트가 `Mesh.ByPointsFaceIndices`의 입력으로 사용됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.VertexIndicesByTri_img.jpg)
