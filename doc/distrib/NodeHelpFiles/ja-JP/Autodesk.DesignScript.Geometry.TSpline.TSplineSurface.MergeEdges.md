## 詳細
次の例では、NURBS 曲線を押し出して T スプライン サーフェスを作成します。そのエッジのうちの 6 つ(形状の両側の 3 つずつ)が `TSplineTopology.EdgeByIndex` ノードを使用して選択されます。エッジの 2 つのセットがサーフェスとともに `TSplineSurface.MergeEdges` ノードに渡されます。エッジ グループの順序は、形状に影響を与えます。エッジの最初のグループは、同じ場所に留まる 2 番目のグループと接するように変位します。`insertCreases` 入力では、結合されたエッジに沿って継ぎ目に折り目を付けるオプションを追加します。結合操作の結果は、プレビューを見やすくするために横に移動されます。
___
## サンプル ファイル

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
