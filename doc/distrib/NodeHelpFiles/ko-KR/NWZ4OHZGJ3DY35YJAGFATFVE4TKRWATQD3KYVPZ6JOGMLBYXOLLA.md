<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## 상세
`Curve.ExtrudeAsSolid (curve, distance)`는 입력 숫자를 사용하여 입력된 닫힌 평면형 곡선을 돌출시켜 돌출의 거리를 결정합니다. 돌출 방향은 곡선이 놓인 평면의 법선 벡터에 의해 결정됩니다. 이 노드는 돌출의 끝을 캡핑하여 솔리드를 만듭니다.

아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 하여 `NurbsCurve.ByPoints` 노드를 사용해 NurbsCurve를 만듭니다. 그런 다음 `Curve.ExtrudeAsSolid` 노드를 사용하여 곡선을 솔리드로 돌출시킵니다. `Curve.ExtrudeAsSolid` 노드에서 `distance`를 입력하는 데 숫자 슬라이더가 사용됩니다.
___
## 예제 파일

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
