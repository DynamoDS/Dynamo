## 상세
`Mesh.TrainglesAsNineNumbers`는 제공된 메쉬의 각 삼각형을 구성하는 정점의 X, Y, Z 좌표를 결정하며 삼각형당 9개의 숫자를 반환합니다. 이 노드는 원래 메쉬를 쿼리, 재구성 또는 변환하는 데 유용할 수 있습니다.

아래 예제에서는 `File Path` 및 `Mesh.ImportFile`을 사용하여 메쉬를 가져옵니다. 그런 다음 `Mesh.TrianglesAsNineNumbers`를 사용하여 각 삼각형의 정점 좌표를 구합니다. 이 리스트는 `lengths` 입력이 3으로 설정된 `List.Chop`을 사용하여 세 개로 세분화됩니다. 그런 다음 `List.GetItemAtIndex`를 사용하여 각 X, Y 및 Z 좌표를 구하고 `Point.ByCoordinates`를 사용하여 정점을 재구성합니다. 점 리스트는 3개(각 삼각형당 3개의 점)로 더 나뉘며 `Polygon.ByPoints`의 입력으로 사용됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)
