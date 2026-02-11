## 상세
`Curve.Extrude (curve, direction)`는 입력 벡터를 사용하여 입력 곡선을 돌출시켜 돌출 방향을 결정합니다. 돌출 거리에는 벡터의 길이가 사용됩니다.

아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 하여 `NurbsCurve.ByControlPoints` 노드를 사용해 NurbsCurve를 만듭니다. `Vector.ByCoordinates 노드의 X, Y 및 Z 구성요소를 지정하는 데는 코드 블록이 사용됩니다. 이 벡터는 `Curve.Extrude` 노드에서 `direction` 입력으로 사용됩니다.
___
## 예제 파일

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
