## 詳細
`Curve.Extrude (curve, distance)` は、入力された数値を使用して押し出す距離を決定し、入力された曲線を押し出します。押し出す方向には、曲線上の法線ベクトルの方向が使用されます。

次の例では、まず `NurbsCurve.ByControlPoints` ノードを使用して、ランダムに生成された一連の点を入力として持つ NURBS 曲線を作成します。次に、`Curve.Extrude` ノードを使用して曲線を押し出します。数値スライダは、`Curve.Extrude` ノードの `distance` 入力に使用されます。
___
## サンプル ファイル

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
