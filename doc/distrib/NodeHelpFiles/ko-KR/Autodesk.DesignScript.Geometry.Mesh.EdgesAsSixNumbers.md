## 상세
`Mesh.EdgesAsSixNumbers`는 제공된 메쉬에서 각각 고유한 모서리를 구성하는 정점의 X, Y, Z 좌표를 결정하며, 따라서 모서리당 6개의 숫자가 반환됩니다. 이 노드는 메쉬 또는 메쉬의 모서리를 쿼리하거나 재구성하는 데 사용할 수 있습니다.

아래 예제에서는 `Mesh.Cuboid`를 사용하여 직육면체 메쉬를 만든 다음, 이를 `Mesh.EdgesAsSixNumbers` 노드에 대한 입력으로 사용하여 6개의 숫자로 표현된 모서리 리스트를 가져옵니다. 이 리스트는 `List.Chop`을 사용하여 6개의 항목으로 구성된 리스트로 세분화됩니다. 그런 다음 `List.GetItemAtIndex` 및 `Point.ByCoordinates`를 사용하여 각 모서리의 시작점 및 끝점 리스트를 재구성합니다. 마지막으로 `List.ByStartPointEndPoint`를 사용하여 메쉬의 모서리를 재구성합니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
