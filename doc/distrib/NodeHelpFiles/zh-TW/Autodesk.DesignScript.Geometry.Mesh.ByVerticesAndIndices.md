## 深入資訊
`Mesh.ByVerticesIndices` 接受一個表示網格三角形 `vertices` 的 `Points` 清單，和一個表示網格如何縫合在一起的 `indices` 清單，建立一個新網格。`vertices` 輸入必須是網格中唯一頂點的展開清單。`indices` 輸入必須是一個整數的展開清單。三個整數一組指定網格中的一個三角形。整數指定頂點在頂點清單中的索引。indices 輸入應從 0 開始編製索引，頂點清單第一點的索引為 0。

以下範例使用 `Mesh.ByVerticesIndices` 節點，透過一個 9 個 `vertices` 的清單和一個 36 個 `indices` 的清單建立網格，為網格的每 12 個三角形指定頂點組合。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByVerticesAndIndices_img.jpg)
