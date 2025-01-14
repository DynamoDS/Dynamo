## 詳細
`Curve.Extrude (curve, direction)` は、入力されたベクトルを使用して押し出す方向を決定し、入力された曲線を押し出します。押し出す距離には、ベクトルの長さが使用されます。

次の例では、まず `NurbsCurve.ByControlPoints` ノードを使用して、ランダムに生成された一連の点を入力として持つ NURBS 曲線を作成します。コード ブロックを使用して、`Vector.ByCoordinates` ノードの X、Y、Z コンポーネントを指定します。このベクトルは、`Curve.Extrude` ノードの `direction` 入力として使用されます。
___
## サンプル ファイル

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
