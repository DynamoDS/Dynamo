## 상세
`Mesh.Cone`은 밑면과 꼭대기 반지름, 높이 및 `divisions` 개수에 대한 입력 값을 사용하여 밑면이 입력 원점을 중심으로 하는 메쉬 원추를 만듭니다. `divisions` 개수는 원추의 꼭대기와 밑면에 생성되는 정점 수와 일치합니다. `divisions` 수가 0이면 Dynamo는 기본값을 사용합니다. Z축을 따르는 분할 수는 항상 5입니다. `cap` 입력은 `Boolean`을 사용하여 원추가 꼭대기에서 닫히는지 여부를 제어합니다.
아래 예제에서는 `Mesh.Cone` 노드를 사용하여 6개의 분할이 있는 원추 모양의 메쉬를 만들며, 따라서 원추의 밑면과 꼭대기는 육각형입니다. `Mesh.Triangles` 노드는 메쉬 삼각형의 분포를 시각화하는 데 사용됩니다.


## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
