<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
頂点の関数の価数は、隣接するエッジの数だけでなく、その周囲の領域における頂点のブレンドに影響を与える仮想グリッド線を考慮します。これにより、変形操作中および微調整操作中に頂点とそのエッジがサーフェスに与える影響をより詳細に理解できるようになります。
通常の頂点と T ポイントで使用すると、`TSplineVertex.FunctionalValence` ノードは「4」の値を返します。これは、サーフェスがグリッドの形状のスプラインによってガイドされることを意味します。「4」以外の関数の価数は、頂点がスター ポイントであり、頂点周囲のブレンドが滑らかでなくなることを意味します。

次の例では、T スプライン平面サーフェスの 2 つの T ポイント頂点で `TSplineVertex.FunctionalValence` が使用されています。`TSplineVertex.Valence` ノードは値 3 を返しますが、選択した頂点の関数の価数は 4 であり、これは T ポイントに固有です。`TSplineVertex.UVNFrame` および `TSplineUVNFrame.Position` ノードは、解析する頂点の位置を視覚化するために使用されます。

## サンプル ファイル

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
