<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## 詳細
`Curve.ExtrudeAsSolid (curve, direction)` は、入力されたベクトルを使用して押し出す方向を決定し、入力された閉じた平面曲線を押し出します。押し出す距離にはベクトルの長さが使用されます。このノードは、押し出しの端点を塞いでソリッドを作成します。

次の例では、まず `NurbsCurve.ByPoints` ノードを使用して、ランダムに生成された一連の点を入力として持つ NURBS 曲線を作成します。コード ブロックを使用して、`Vector.ByCoordinates` ノードの X、Y、Z コンポーネントを指定します。このベクトルは、`Curve.ExtrudeAsSolid` ノードの方向の入力として使用されます。
___
## サンプル ファイル

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
