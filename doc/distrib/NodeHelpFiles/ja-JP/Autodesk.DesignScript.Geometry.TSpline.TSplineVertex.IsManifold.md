## In-Depth
次の例では、内部エッジを共有する 2 つのサーフェスを結合して非多様体サーフェスを作成します。その結果、明確な前面と背面がないサーフェスが作成されます。非多様体サーフェスは修復されるまでボックス モードでのみ表示できます。サーフェスのすべての頂点のクエリーを実行するために `TSplineTopology.DecomposedVertices` を使用し、多様体とみなされる頂点をハイライト表示するために `TSplineVertex.IsManifold` ノードを使用しています。`TSplineVertex.UVNFrame` ノードと `TSplineUVNFrame.Position` ノードを使用して、非多様体頂点が抽出され、その位置が視覚化されます。


## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold_img.jpg)
