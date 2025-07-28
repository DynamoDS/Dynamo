## 詳細
T スプライン サーフェスは、すべての T ポイントがスター ポイントから少なくとも 2 つのアイソカーブによって分離されている場合に標準です。T スプライン サーフェスを NURBS サーフェスに変換するには、標準化が必要です。

次の例では、`TSplineSurface.ByBoxLengths` を使用して生成された T スプライン サーフェスの面の 1 つが再分割されます。`TSplineSurface.IsStandard` を使用してサーフェスが標準であるかどうかを確認しますが、負の結果が生成されます。
次に、`TSplineSurface.Standardize` を使用してサーフェスを標準化します。新しい制御点は、サーフェスの形状を変更せずに導入されます。作成されるサーフェスを `TSplineSurface.IsStandard` を使用して確認すると、これが標準となったことが確認されます。
ノード `TSplineFace.UVNFrame` と `TSplineUVNFrame.Position` は、サーフェスで再分割された面をハイライト表示するために使用されます。
___
## サンプル ファイル

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
