## 상세
`Mesh.Sphere`는 입력 `origin` 점을 중심으로 지정된 `radius` 및 `divisions` 수에 따라 메시 구를 만듭니다. `icosphere` 부울 입력은 `icosphere`와 `UV-Sphere` 구형 메쉬 유형 간에 전환하는 데 사용됩니다. 아이코스피어 메쉬는 UV 메쉬보다 더 규칙적인 삼각형으로 구를 덮으며 다운스트림 모델링 작업에서 더 나은 결과를 제공하는 경향이 있습니다. UV 메쉬의 극은 구 축과 정렬되며 삼각형 레이어는 축을 중심으로 세로 방향으로 생성됩니다.

아이코스피어의 경우 구 축 주위의 삼각형 수는 지정된 분할 수만큼 적을 수 있고 최대 두 배에 달할 수도 있습니다. `UV-sphere`의 분할에 따라 구 주위에 세로 방향으로 생성되는 삼각형 레이어 수가 결정됩니다. `divisions` 입력을 0 으로 설정하면 노드는 메쉬 유형에 관계없이 32개의 분할(기본값)을 가진 UV-구를 반환합니다.

아래 예제에서는 `Mesh.Sphere` 노드를 사용하여 반지름과 분할이 동일한 두 개의 구를 만듭니다. 하지만 각각 다른 방법을 사용하여 구를 생성합니다. `icosphere` 입력을 `True`로 설정하면 `Mesh.Sphere`는 `icosphere`를 반환합니다. 또는 `icosphere` 입력을 `False`로 설정하면 `Mesh.Sphere` 노드는 `UV-sphere`를 반환합니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.Sphere_img.jpg)
