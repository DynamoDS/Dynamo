## 詳細
閉じた NURBS 曲線を別のシステム(Alias など)に書き出す必要がある場合、またはシステムが周期的に形成する曲線を想定している場合は、`NurbsCurve.PeriodicKnots` を使用します。多くの CAD ツールでは、ラウンド トリップの精度にこの形成を想定しています。

`PeriodicKnots` は、*周期的に*(未固定で)形成するノット ベクトルを返します。`Knots` は*固定*で形成するノット ベクトルを返します。どちらの配列も同じ長さです。これらは同じ曲線を記述する 2 つの異なる方法です。固定の形成では、ノットが始点と終点で繰り返されるため、曲線がパラメータ範囲に固定されます。周期的な形成では、ノットの間隔は始点と終点で繰り返され、滑らかな閉じたループになります。

次の例では、`NurbsCurve.ByControlPointsWeightsKnots` を使用して周期的な NURBS 曲線を作成します。Watch ノードは、`Knots` と `PeriodicKnots` を比較するため、同じ長さでも異なる値を持つことを確認できます。Knots は固定の形成(端点で繰り返されるノット)で、PeriodicKnots は、曲線の周期性を定義する繰返しの異なるパターンを持つ未固定の形成です。
___
## サンプル ファイル

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
