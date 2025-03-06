## In-Depth
`TSplineVertex.IsTPoint` は、頂点が T ポイントであるかどうかを返します。T ポイントは、制御点の一部の行の端部にある頂点です。

次の例では、T スプライン ボックス プリミティブで `TSplineSurface.SubdivideFaces` を使用して、T ポイントをサーフェスに追加する複数の方法の 1 つを示しています。インデックスの頂点が T ポイントであることを確認するには、`TSplineVertex.IsTPoint` ノードを使用します。T ポイントの位置をより適切に視覚化するには、`TSplineVertex.UVNFrame` ノードと `TSplineUVNFrame.Position` ノードを使用します。



## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
