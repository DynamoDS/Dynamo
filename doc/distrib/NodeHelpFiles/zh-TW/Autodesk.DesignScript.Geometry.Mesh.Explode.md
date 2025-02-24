## 深入資訊
`Mesh.Explode` 節點接受單一網格，然後傳回網格面清單作為獨立網格。

以下範例顯示使用 `Mesh.Explode` 分解的網格圓頂，後面接著每個面沿面法線方向的偏移。這是使用 `Mesh.TriangleNormals` 和 `Mesh.Translate` 節點完成。儘管在此範例中，網格面看起來像是四邊形，但它們實際上是具有相同法線的三角形。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.Explode_img.jpg)
