## In-Depth
`TSplineVertex.Index` は、T スプライン サーフェス上で選択した頂点のインデックス番号を返します。T スプライン サーフェス トポロジでは、面、エッジ、頂点のインデックスは、リスト内の項目のシーケンス番号と必ずしも一致しないことに注意してください。この問題に対処するには、ノード `TSplineSurface.CompressIndices` を使用します。

次の例では、ボックスの形状の T スプライン プリミティブで `TSplineTopology.StarPointVertices` が使用されています。次に、`TSplineVertex.Index` を使用してスターポイントの頂点のインデックスのクエリーを実行し、さらに編集するために、選択した頂点を `TSplineTopology.VertexByIndex` が返します。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
