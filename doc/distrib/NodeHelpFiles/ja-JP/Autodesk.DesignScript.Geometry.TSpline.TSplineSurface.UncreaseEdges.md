## In-Depth
`TSplineSurface.CreaseEdges` ノードとは逆に、このノードは T スプライン サーフェス上の指定したエッジの折り目を削除します。
次の例では、T スプライン サーフェスが T スプライン トーラスから生成されます。すべてのエッジは、`TSplineTopology.EdgeByIndex` ノードと `TSplineTopology.EdgesCount` ノードを使用して選択され、`TSplineSurface.CreaseEdges` ノードを使用してすべてのエッジに折り目が適用されます。次に、インデックス 0 ～ 7 のエッジのサブセットが選択され、今回は `TSplineSurface.UncreaseEdges` ノードを使用して逆の操作が適用されます。選択したエッジの位置は `TSplineEdge.UVNFrame` ノードと `TSplineUVNFrame.Poision` ノードを使用してプレビューします。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
