## In-Depth
`TSplineEdge.Info` は、T スプライン サーフェス エッジの次のプロパティを返します。
- `uvnFrame`: T スプライン エッジのハル上の点、U ベクトル、V ベクトル、法線ベクトル
- `index`: エッジのインデックス
- `isBorder`: 選択したエッジが T スプライン サーフェスの境界であるかどうか
- `isManifold`: 選択したエッジが多様体であるかどうか

次の例では、`TSplineTopology.DecomposedEdges` を使用して T スプライン円柱プリミティブ サーフェスのすべてのエッジのリストを取得し、`TSplineEdge.Info` を使用してそれらのプロパティを調べます。


## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
