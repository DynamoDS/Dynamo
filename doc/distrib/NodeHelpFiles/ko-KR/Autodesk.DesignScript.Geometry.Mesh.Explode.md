## 상세
`Mesh.Explode` 노드는 단일 메쉬를 사용하고 메쉬 면 리스트를 독립 메쉬로 반환합니다.

아래 예제에서는 `Mesh.Explode`를 사용하여 메쉬 돔을 분해한 다음 면 법선 방향으로 간격띄우기한 각 면을 보여줍니다. 이는 `Mesh.TriangleNormals` 및 `Mesh.Translate` 노드를 사용하여 구현됩니다. 이 예제에서는 메쉬 면이 사각형으로 보이지만 실제로는 동일한 법선을 가진 삼각형입니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
