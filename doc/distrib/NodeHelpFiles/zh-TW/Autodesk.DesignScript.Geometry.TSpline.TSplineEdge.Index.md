## In-Depth
請注意，在 T 雲形線曲面拓樸中，`Face`、`Edge` 和 `Vertex` 的索引不一定會與清單中項目的序列號碼重合。請使用 `TSplineSurface.CompressIndices` 節點解決此問題。

以下範例使用 `TSplineTopology.DecomposedEdges` 擷取 T 雲形線曲面的邊界邊，然後使用 `TSplineEdge.Index` 節點取得所提供邊的索引。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
