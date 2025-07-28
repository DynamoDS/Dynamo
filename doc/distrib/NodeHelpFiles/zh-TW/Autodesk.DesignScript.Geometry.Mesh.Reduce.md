## 深入資訊
`Mesh.Reduce` 會建立一個減少三角形數的新網格。`triangleCount` 輸入定義輸出網格的目標三角形數。請注意，如果出現極端的 `triangleCount` 目標值，`Mesh.Reduce` 可能會顯著改變網格的形狀。在以下範例中，使用 `Mesh.ImportFile` 匯入網格，然後透過 `Mesh.Reduce` 節點減少三角形數並平移到其他位置，方便獲得更好的預覽效果和進行比較。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.Reduce_img.jpg)
