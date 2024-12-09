<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## 詳細
Curve.ExtrudeAsSolid (direction, distance) は、入力されたベクトルを使用して押し出す方向を決定し、入力された閉じた平面曲線を押し出します。押し出す距離には別の `distance` 入力が使用されます。このノードは、押し出しの端点を塞いでソリッドを作成します。

次の例では、まず `NurbsCurve.ByPoints` ノードを使用して、ランダムに生成された一連の点を入力として持つ NURBS 曲線を作成します。`code block` を使用して`Vector.ByCoordinates` ノードの X、Y、Z コンポーネントを指定します。このベクトルは、`Curve.ExtrudeAsSolid` ノードの方向の入力として使用され、同時に数値スライダは `distance` 入力のコントロールに使用されます。
___
## サンプル ファイル

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
