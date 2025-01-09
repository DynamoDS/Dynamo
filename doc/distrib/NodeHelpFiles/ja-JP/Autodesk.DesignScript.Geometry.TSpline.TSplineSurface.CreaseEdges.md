## In-Depth
`TSplineSurface.CreaseEdges` は、T スプライン サーフェス上の指定されたエッジに鋭角な折り目を追加します。
次の例では、T スプライン サーフェスが T スプライン トーラスから生成されます。`TSplineTopology.EdgeByIndex` ノードを使用してエッジを選択し、`TSplineSurface.CreaseEdges` ノードを使用してそのエッジに折り目を適用します。そのエッジの両側のエッジの頂点にも折り目が付きます。選択したエッジの位置は、`TSplineEdge.UVNFrame` ノードと `TSplineUVNFrame.Poision` ノードを使用してプレビューします。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
