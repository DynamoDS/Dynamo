## In-Depth
如果輸入的 T 雲形線邊為邊界，則 `TSplineEdge.IsBorder` 會傳回 `True`。

以下範例研究兩個 T 雲形線曲面的邊。曲面是一個圓柱及其增厚的版本。若要選取所有邊，這兩種情況都會使用 `TSplineTopology.EdgeByIndex` 節點，並使用索引輸入 - 從 0 到 n 的整數範圍，其中 n 是 `TSplineTopology.EdgesCount` 提供的邊數。這是使用 `TSplineTopology.DecomposedEdges` 直接選取邊的替代方法。如果是增厚的圓柱，還會使用 `TSplineSurface.CompressIndices` 將邊索引重新排序。
使用 `TSplineEdge.IsBorder` 節點檢查哪些邊是邊界邊。使用 `TSplineEdge.UVNFrame` 和 `TSplineUVNFrame.Position` 節點亮顯平面圓柱的邊界邊位置。增厚圓柱沒有邊界邊。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
