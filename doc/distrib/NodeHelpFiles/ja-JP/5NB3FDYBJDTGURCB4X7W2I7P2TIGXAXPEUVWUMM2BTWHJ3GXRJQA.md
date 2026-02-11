<!--- Autodesk.DesignScript.Geometry.Curve.Extrude(curve, direction, distance) --->
<!--- 5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA --->
## 詳細
`Curve.Extrude (curve, direction, distance)` は、入力されたベクトルを使用して押し出す方向を決定し、入力された曲線を押し出します。押し出す距離には別の `distance` 入力が使用されます。

次の例では、まず `NurbsCurve.ByControlPoints` ノードを使用して、ランダムに生成された一連の点を入力として持つ NURBS 曲線を作成します。コード ブロックを使用して、`Vector.ByCoordinates` ノードの X、Y、Z コンポーネントを指定します。このベクトルは、`Curve.Extrude` ノードの方向の入力として使用され、同時に `number slider` を使用して `distance` 入力をコントロールします。
___
## サンプル ファイル

![Curve.Extrude(curve, direction, distance)](./5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA_img.jpg)
