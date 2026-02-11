## In-Depth
與 `TSplineSurface.CreaseEdges` 節點相反，此節點會移除 T 雲形線曲面上指定邊的縐摺。
以下範例從 T 雲形線圓環產生 T 雲形線曲面。使用 `TSplineTopology.EdgeByIndex` 和 `TSplineTopology.EdgesCount` 節點選取所有邊，並透過 `TSplineSurface.CreaseEdges` 節點將縐摺作用到所有邊。然後選取索引為 0 到 7 的一組邊，這次使用 `TSplineSurface.UncreaseEdges` 節點套用反轉作業。透過 `TSplineEdge.UVNFrame` 和 `TSplineUVNFrame.Poision` 節點預覽所選邊的位置。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
