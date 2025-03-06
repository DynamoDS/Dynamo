## In-Depth
此節點會傳回 TSplineUVNFrame 物件，此物件可用於顯示頂點位置和方位，以及使用 U、V 或 N 向量進一步操控 T 雲形線曲面。

以下範例使用 `TSplineVertex.UVNFrame` 節點取得所選頂點的 UVN 框，然後使用 UVN 框傳回頂點的法線。最後使用 `TSplineSurface.MoveVertices` 節點沿法線方向移動頂點。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.UVNFrame_img.jpg)
