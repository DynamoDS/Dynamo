## 深入資訊
`Mesh.TrainglesAsNineNumbers` 決定提供網格中組成每個三角形之頂點的 X、Y 和 Z 座標，每個三角形會產生九個數字。此節點在查詢、重新建構或轉換原始網格時很有用。

在以下範例中，使用 `File Path` 和 `Mesh.ImportFile` 匯入網格。然後使用 `Mesh.TrianglesAsNineNumbers` 得到每個三角形的頂點座標。接著使用 `List.Chop`，將 `lengths` 輸入設定為 3，將此清單細分成三個。再使用 `List.GetItemAtIndex` 取得每個 X、Y 和 Z 座標，並使用 `Point.ByCoordinates` 重建頂點。進一步將點清單分成三個 (每個三角形有 3 個點)，作為 `Polygon.ByPoints` 的輸入。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.TrianglesAsNineNumbers_img.jpg)
