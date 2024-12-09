## In-Depth
`TSplineVertex.Index` 會傳回 T 雲形線曲面上所選頂點的索引號碼。請注意，在 T 雲形線曲面拓樸中，面、邊和頂點的索引不一定會與清單中項目的序列號碼重合。請使用 `TSplineSurface.CompressIndices` 節點解決此問題。

以下範例對方塊形狀的 T 雲形線基本型使用 `TSplineTopology.StarPointVertices`，然後使用 `TSplineVertex.Index` 查詢 Y 點頂點的索引，`TSplineTopology.VertexByIndex` 會傳回選取的頂點以供進一步編輯。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
