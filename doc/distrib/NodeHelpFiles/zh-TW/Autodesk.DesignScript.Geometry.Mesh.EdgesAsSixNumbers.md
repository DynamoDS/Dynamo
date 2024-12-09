## 深入資訊
`Mesh.EdgesAsSixNumbers` 決定提供網格中組成每條唯一邊之頂點的 X、Y 和 Z 座標，因此每條邊會產生六個數字。使用此節點可查詢或重新建構網格或網格的邊。

以下範例使用 `Mesh.Cuboid` 建立立方體網格，然後使用該立方體當作 `Mesh.EdgesAsSixNumbers` 節點的輸入，取得以六個數字表示的邊清單。使用 `List.Chop` 將清單細分為 6 個項目的清單，然後使用 `List.GetItemAtIndex` 和 `Point.ByCoordinates` 重新建構每條邊的起點清單和終點清單。最後使用 `List.ByStartPointEndPoint` 重新建構網格的邊。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.EdgesAsSixNumbers_img.jpg)
