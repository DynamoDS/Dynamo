## In-Depth
`TSplineEdge.IsBorder` は、入力 T スプライン エッジが境界の場合に `True` を返します。

次の例では、2 つの T スプライン サーフェスのエッジを調査します。サーフェスは円柱と厚みを付けたバージョンの円柱です。すべてのエッジを選択するには、どちらの場合でも `TSplineTopology.EdgeByIndex` ノードを使用します。インデックス入力は 0 ～ n の範囲の整数で、n は `TSplineTopology.EdgesCount` によって指定されるエッジの数です。これは、`TSplineTopology.DecomposedEdges` を使用して直接エッジを選択する代わりの方法です。厚みを付けた円柱の場合は、エッジ インデックスを並べ替えるために、`TSplineSurface.CompressIndices` も使用されます。
`TSplineEdge.IsBorder` ノードは、どのエッジが境界エッジであるかを確認するために使用されます。`TSplineEdge.UVNFrame` ノードと `TSplineUVNFrame.Position` ノードを使用して、平坦な円柱の境界エッジの位置をハイライト表示します。厚みを付けた円柱には境界エッジがありません。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
