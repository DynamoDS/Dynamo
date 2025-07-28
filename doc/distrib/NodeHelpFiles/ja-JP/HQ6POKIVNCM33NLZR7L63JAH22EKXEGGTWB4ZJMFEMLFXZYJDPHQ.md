<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderVertices --->
<!--- HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ --->
## 詳細
`TSplineTopology.BorderVertices` は、T スプライン サーフェスに含まれる境界頂点のリストを返します。

次の例では、`TSplineSurface.ByCylinderPointsRadius` を使用して 2 つの T スプライン サーフェスを作成します。1 つは開いたサーフェスで、もう 1 つは `TSplineSurface.Thicken` を使用して厚みが付けられ、その結果、閉じたサーフェスになります。両方を `TSplineTopology.BorderVertices` ノードを使用して調べると、1 つ目の方は境界頂点のリストを返しますが、2 つ目の方は空のリストを返します。これは、サーフェスが囲まれていることで境界頂点がないためです。
___
## サンプル ファイル

![TSplineTopology.BorderVertices](./HQ6POKIVNCM33NLZR7L63JAH22EKXEGGTWB4ZJMFEMLFXZYJDPHQ_img.jpg)
