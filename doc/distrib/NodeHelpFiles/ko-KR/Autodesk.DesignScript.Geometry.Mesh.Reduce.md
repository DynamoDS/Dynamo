## 상세
`Mesh.Reduce`는 삼각형 수를 줄여 새 메쉬를 만듭니다. `triangleCount` 입력은 출력 메쉬의 목표 삼각형 수를 정의합니다. `Mesh.Reduce`는 매우 극단적인 `triangleCount` 목표의 경우 메쉬의 모양을 크게 변경할 수 있습니다. 아래 예제에서 `Mesh.ImportFile`은 메쉬를 가져오는 데 사용되며, 가져온 메쉬는 `Mesh.Reduce` 노드로 축소되고 더 나은 미리보기 및 비교를 위해 다른 위치로 변환됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.Reduce_img.jpg)
