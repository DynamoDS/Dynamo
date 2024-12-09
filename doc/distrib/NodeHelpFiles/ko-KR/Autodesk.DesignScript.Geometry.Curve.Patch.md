## 상세
Patch는 입력 곡선을 경계로 사용하여 표면을 작성하려고 합니다. 입력 곡선은 닫혀 있어야 합니다. 아래 예에서는 먼저 Point.ByCylindricalCoordinates 노드를 사용하여 원에 설정된 간격에 맞춰 점 세트를 작성합니다. 이때, 높이 및 반지름은 임의의 값을 사용합니다. 그런 다음 NurbsCurve.ByPoints 노드를 사용하여 이러한 점을 기준으로 닫힌 곡선을 작성합니다. Patch 노드는 닫힌 곡선의 경계에서 표면을 작성하는 데 사용됩니다. 점이 임의 반지름 및 높이를 사용하여 작성되었으므로 일부 정렬의 경우 패치할 수 없는 곡선이 작성됩니다.
___
## 예제 파일

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

