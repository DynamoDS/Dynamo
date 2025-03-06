<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## 詳細
`Curve.NormalAtParameter (curve, param)` は、曲線の指定されたパラメータで法線方向に位置合わせされたベクトルを返します。曲線はパラメータ化され、0 ～ 1 の範囲で計測されます。0 は曲線の始点、1 は曲線の終点を表します。

次の例では、まず `NurbsCurve.ByControlPoints` ノードを使用して、ランダムに生成された一連の点を入力として持つ NURBS 曲線を作成します。0 ～ 1 の範囲に設定された数値スライダを使用して、`Curve.NormalAtParameter` ノードの`parameter` 入力をコントロールします。
___
## サンプル ファイル

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
