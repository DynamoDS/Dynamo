<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## 詳細
`Curve.SweepAsSolid` は、入力された閉じたプロファイル曲線を、指定されたパスに沿ってスイープしてソリッドを作成します。

次の例では、長方形を基準のプロファイル曲線として使用します。パスの作成には余弦関数を使用し、角度のシーケンスで点のセットの X 座標を変化させています。この点群が、`NurbsCurve.ByPoints` ノードへの入力として使用されます。次に、`Curve.SweepAsSolid` ノードで作成された余弦曲線に沿って、長方形をスイープしてソリッドを作成しています。
___
## サンプル ファイル

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
