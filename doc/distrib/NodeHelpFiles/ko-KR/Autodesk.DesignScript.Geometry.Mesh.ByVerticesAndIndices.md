## 상세
`Mesh.ByVerticesIndices`는 메쉬 삼각형의 `vertices`를 나타내는 `Points` 리스트와 메쉬가 함께 스티칭되는 방식을 나타내는 `indices` 리스트를 사용하여 새 메쉬를 만듭니다. `vertices` 입력은 메쉬의 고유한 정점으로 구성된 단순 리스트여야 합니다. `indices` 입력은 정수의 단순 리스트여야 합니다. 3개의 정수로 구성된 각 세트는 메쉬에서 삼각형을 지정합니다. 정수는 정점 리스트에 있는 정점의 색인을 지정합니다. 색인 입력은 0부터 시작해야 하며, 정점 리스트의 첫 번째 점은 색인이 0이어야 합니다.

아래 예제에서는 `Mesh.ByVerticesIndices` 노드를 사용하여 9개의 `vertices`로 이루어진 리스트와 36개의 `indices`로 이루어진 리스트로 메쉬를 생성하고, 메쉬의 12개의 삼각형 각각에 대한 정점 조합을 지정합니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices_img.jpg)
