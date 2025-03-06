<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## 상세
`Curve.ExtrudeAsSolid (curve, direction)`는 입력 벡터를 사용하여 입력된 닫힌 평면형 곡선을 돌출시켜 돌출 방향을 결정합니다. 돌출 거리에는 벡터의 길이가 사용됩니다. 이 노드는 돌출의 끝을 캡핑하여 솔리드를 만듭니다.

아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 하여 `NurbsCurve.ByPoints` 노드를 사용해 NurbsCurve를 만듭니다. `Vector.ByCoordinates 노드의 X, Y 및 Z 구성요소를 지정하는 데는 코드 블록이 사용됩니다. 이 벡터는 `Curve.ExtrudeAsSolid` 노드의 방향 입력으로 사용됩니다.
___
## 예제 파일

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
