## 詳細
閉じた NURBS 曲線を別のシステム(Alias など)に書き出す必要がある場合、またはシステムが周期的に形成する曲線を想定している場合は、`NurbsCurve.PeriodicControlPoints` を使用します。多くの CAD ツールでは、ラウンド トリップの精度にこの形成を想定しています。

`PeriodicControlPoints` は、*周期的に*形成する制御点を返します。`ControlPoints` は、*固定*で形成する制御店を返します。どちらの配列も同じ数の点を持ちます。これらは同じ曲線を記述する 2 つの異なる方法です。周期的な形成では、最後の数個の制御点が最初の数個(曲線の次数と同じ数)と一致するため、曲線は滑らかに閉じます。固定の形成は異なるレイアウトを使用するため、2 つの配列で点の位置が異なります。

次の例では、`NurbsCurve.ByControlPointsWeightsKnots` を使用して周期的な NURBS 曲線を作成します。Watch ノードは、`ControlPoints` と `PeriodicControlPoints` を比較するため、同じ長さでも異なる点の位置を確認できます。ControlPoint は赤色で表示されるため、バックグラウンド プレビューで黒で表示される PeriodicControlPoints と区別することができます。
___
## サンプル ファイル

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
