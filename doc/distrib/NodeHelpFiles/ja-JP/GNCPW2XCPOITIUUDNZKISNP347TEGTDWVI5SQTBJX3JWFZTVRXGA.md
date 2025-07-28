<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## 詳細
次の例では、押し出され、再分割され、プルされた頂点と面を持つ平面 T スプライン サーフェスが `TSplineTopology.DecomposedVertices` ノードで検査され、T スプライン サーフェスに含まれる次のタイプの頂点のリストを返します。

- `all`: すべての頂点のリスト
- `regular`: 通常の頂点のリスト
- `tPoints`: T ポイント頂点のリスト
- `starPoints`: スター ポイント頂点のリスト
- `nonManifold`: 非多様体頂点のリスト
- `border`: 境界頂点のリスト
- `inner`: 内側の頂点のリスト

ノード `TSplineVertex.UVNFrame` と `TSplineUVNFrame.Position` は、サーフェスのさまざまなタイプの頂点をハイライト表示するために使用されます。

___
## サンプル ファイル

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
