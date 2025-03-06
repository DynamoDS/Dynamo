## In-Depth
`TSplineVertex.Info` は、T スプライン頂点の次のプロパティを返します。
- `uvnFrame`: T スプライン頂点のハル上の点、U ベクトル、V ベクトル、法線ベクトル
- `index`: T スプライン サーフェス上の選択した頂点のインデックス
- `isStarPoint`: 選択した頂点がスター ポイントであるかどうか
- `isTpoint`: 選択した頂点が T ポイントであるかどうか
- `isManifold`: 選択した頂点が多様体であるかどうか
- `valence`: 選択した T スプライン頂点上のエッジ数
- `functionalValence`: 頂点の関数の価数。詳細については、`TSplineVertex.FunctionalValence` ノードのドキュメントを参照してください。

次の例では、T スプライン サーフェスを作成し、その頂点を選択するために、それぞれ `TSplineSurface.ByBoxCorners` と `TSplineTopology.VertexByIndex` を使用しています。`TSplineVertex.Info` は、選択した頂点に関する上記の情報を収集するために使用します。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
