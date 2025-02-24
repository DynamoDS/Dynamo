## 상세
`Curve.Extrude (curve, distance)`는 입력 숫자를 사용하여 입력 곡선을 돌출시켜 돌출 거리를 결정합니다. 돌출 방향에는 곡선을 따른 법선 벡터의 방향이 사용됩니다.

아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 하여 `NurbsCurve.ByControlPoints` 노드를 사용해 NurbsCurve를 만듭니다. 그런 다음 `Curve.Extrude` 노드를 사용하여 곡선을 돌출시킵니다. 숫자 슬라이더는 `Curve.Extrude` 노드의 `distance`를 입력하는 데 사용됩니다.
___
## 예제 파일

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
