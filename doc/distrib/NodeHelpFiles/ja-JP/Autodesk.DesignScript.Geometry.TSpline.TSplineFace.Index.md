## In-Depth
`TSplineFace.Index` は、T スプライン サーフェス上の面のインデックスを返します。T スプライン サーフェス トポロジでは、面、エッジ、頂点のインデックスが、必ずしもリスト内の項目のシーケンス番号と一致しないことに注意してください。この問題に対処するには、ノード `TSplineSurface.CompressIndices` を使用します。

次の例では、`TSplineFace.Index` を使用して T スプライン サーフェスのすべての通常の面のインデックスを表示します。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Index_img.jpg)
