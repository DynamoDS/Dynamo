## In-Depth
`TSplineSurface.BevelEdges` 節點會將選取的一邊或一組邊沿著面的兩個方向偏移，並以形成通道的一系列邊來取代原始邊。

以下範例使用 T 雲形線方塊基本型的一組邊作為 `TSplineSurface.BevelEdges` 節點的輸入。此範例說明以下輸入如何影響結果:
- `percentage` 控制新建立的邊沿相鄰面的分佈，值越接近 0，新邊越靠近原始邊，值越接近 1，則越遠離原始邊。
- `numberOfSegments` 控制通道中新面的數目。
- `keepOnFace` 定義斜邊是否放在原始面的平面。如果值設定為 True，roundness 輸入將無效。
- `roundness` 控制斜切的圓度，應為 0 到 1 之間的值，0 會產生直角的斜切，1 會傳回圓角的斜切。

有時會開啟方塊模式，更清楚得知形狀為何。


## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
