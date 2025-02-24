## 詳細
`TSplineTopology.BorderEdges` は、T スプライン サーフェスに含まれる境界エッジのリストを返します。

次の例では、`TSplineSurface.ByCylinderPointsRadius` を使用して 2 つの T スプライン サーフェスを作成します。1 つは開いたサーフェスで、もう 1 つは `TSplineSurface.Thicken` を使用して厚みが付けられ、これにより閉じたサーフェスになります。`TSplineTopology.BorderEdges` ノードを使用して両方を調べると、1 つ目の方は境界エッジのリストを返しますが、2 つ目の方は空のリストを返します。これは、サーフェスが囲まれていることで境界エッジがないためです。
___
## サンプル ファイル

![TSplineTopology.BorderEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges_img.jpg)
