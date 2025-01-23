<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## 深入資訊
`TSplineSurface.CompressIndexes` 節點會移除 T 雲形線曲面之邊、頂點或面的索引數因為各種作業 (例如「刪除面」) 產生的間隙。會保留索引的順序。

以下範例從影響形狀邊索引的四角球基本型曲面中刪除多個面。使用 `TSplineSurface.CompressIndexes` 修復形狀的邊索引，如此就可以選取具有索引 1 的邊。

## 範例檔案

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
