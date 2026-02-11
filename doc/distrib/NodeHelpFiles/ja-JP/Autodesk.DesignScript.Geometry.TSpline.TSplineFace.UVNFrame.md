## In-Depth
面の UVNFrame は、法線ベクトルと UV の方向を返すことによって、面の位置と方向に関する有益な情報を提供します。
次の例では、`TSplineFace.UVNFrame` ノードを使用してクワッドボール プリミティブ上の面の分布を視覚化します。`TSplineTopology.DecomposedFaces` ノードを使用してすべての面のクエリーを実行し、`TSplineFace.UVNFrame` ノードを使用して面の図心の位置を点として取得します。点は、`TSplineUVNFrame.Position` ノードを使用して視覚化します。ノードの右クリックメニューで[ラベルを表示]を有効にすると、ラベルがバックグラウンドのプレビューに表示されます。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)
