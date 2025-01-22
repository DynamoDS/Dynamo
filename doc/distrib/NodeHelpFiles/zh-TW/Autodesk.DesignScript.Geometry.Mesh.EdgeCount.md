## 深入資訊
此節點會計算提供網格中的邊數。如果網格由三角形組成 (`MeshToolkit` 中的所有網格都是這種情況)，`Mesh.EdgeCount` 節點只會傳回唯一的邊。因此，邊數應該不會是網格中三角形數的三倍。使用此假設可以確認網格不會包含任何未接合的面 (但可能出現在匯入的網格中)。

在以下範例中，使用 `Mesh.Cone` 和 `Number.Slider` 建立圓錐，然後使用該圓錐當作計算邊數的輸入。使用 `Mesh.Edges` 和 `Mesh.Triangles` 兩者可預覽網格的結構和格線，如果是複雜和大量的網格，`Mesh.Edges` 的效能更好。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgeCount_img.jpg)
