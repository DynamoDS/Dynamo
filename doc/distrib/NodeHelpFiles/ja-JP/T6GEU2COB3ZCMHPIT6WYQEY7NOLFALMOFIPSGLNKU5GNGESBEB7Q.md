<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## 詳細
`NurbsCurve.ByControlPointsWeightsKnots` を使用すると、NURBS 曲線の重みとノットを手動でコントロールできます。重みのリストは、制御点のリストと同じ長さにする必要があります。ノットのリストのサイズは、制御点の数 + 次数 + 1 の値と同じにする必要があります。

次の例では、まず一連のランダムな点間を補間して NURBS 曲線を作成します。ノット、重み、制御点を使用して、曲線の対応する部分を求めます。重みのリストを変更するには、`List.ReplaceItemAtIndex` を使用します。最後に、`NurbsCurve.ByControlPointsWeightsKnots` を使用して、変更した重みを持つ NURBS 曲線を再作成します。

___
## サンプル ファイル

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

