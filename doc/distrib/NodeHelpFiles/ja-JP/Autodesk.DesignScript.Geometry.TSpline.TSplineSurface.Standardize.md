## 詳細
`TSplineSurface.Standardize` ノードは、T スプライン サーフェスを標準化するために使用されます。
標準化とは、NURBS 変換のために T スプライン サーフェスを準備することを意味し、少なくとも 2 つのアイソカーブによってスター ポイントから分離されるまで、すべての T ポイントを延長することです。標準化は、サーフェスの形状を変更しませんが、サーフェスの NURBS 互換を実現するために必要なジオメトリ要件を満たす制御点を追加する場合があります。

次の例では、`TSplineSurface.ByBoxLengths` を使用して生成された T スプライン サーフェスの面の 1 つが再分割されています。
サーフェスが標準であるかどうかを確認するために `TSplineSurface.IsStandard` ノードを使用しますが、負の結果が生成されます。
次に、`TSplineSurface.Standardize` を使用してサーフェスを標準化します。結果のサーフェスは `TSplineSurface.IsStandard` を使用してチェックされ、これが標準であることが確認されます。
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## サンプル ファイル

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
