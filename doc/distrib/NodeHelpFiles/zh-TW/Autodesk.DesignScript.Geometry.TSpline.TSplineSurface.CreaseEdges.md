## In-Depth
`TSplineSurface.CreaseEdges` 會在 T 雲形線曲面上的指定邊新增銳利的縐摺。
以下範例從 T 雲形線圓環產生 T 雲形線曲面。使用 `TSplineTopology.EdgeByIndex` 節點選取一條邊，並透過 `TSplineSurface.CreaseEdges` 節點將縐摺作用到該條邊。這條邊兩邊上的頂點也會變成縐摺。透過 `TSplineEdge.UVNFrame` 和 `TSplineUVNFrame.Poision` 節點預覽所選邊的位置。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
