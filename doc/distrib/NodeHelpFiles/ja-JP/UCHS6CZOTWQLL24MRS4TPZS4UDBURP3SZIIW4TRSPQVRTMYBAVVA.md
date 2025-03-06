<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
次の例では、平面プリミティブのコーナー頂点でノード `TSplineSurface.UncreaseVertices` が使用されます。既定では、これらの頂点はサーフェスの作成時に折り目が付けられます。`TSplineVertex.UVNFrame` ノードと `TSplineUVNFrame.Poision` ノードを使用し、`Show Labels` オプションを有効にして、頂点を識別します。次に `TSplineTopology.VertexByIndex` ノードを使用してコーナー頂点を選択し、折り目を解除します。形状がスムーズ モードでプレビューされている場合、このアクションの効果をプレビューできます。

## サンプル ファイル

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
