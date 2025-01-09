## 상세
`Mesh.Cuboid`는 입력 점을 중심으로 하고 지정된 `width`, `length` 및  `height`와 X, Y, Z 방향을 따라 지정된 분할 수를 사용하여 메쉬 직육면체를 만듭니다. 분할 개수가 명시적으로 지정되지 않았거나 입력 `xDivisions`, `yDivisions` 또는 `zDivisions` 중 어느 하나라도 0인 경우 모든 방향에 대해 기본값인 5개의 분할이 사용됩니다.
아래 예제에서는 `Mesh.Cuboid` 노드를 사용하여 직육면체 메쉬를 만들고 `Mesh.Triangles` 노드를 사용하여 메쉬 삼각형의 분포를 시각화합니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cuboid_img.jpg)
