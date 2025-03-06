## 深入資訊
`Mesh.PlaneCut` 會傳回一個被給定平面切割的網格。切割的結果是平面在 `plane` 輸入之法線方向那一側的網格部分。`makeSolid` 參數控制是否將網格視為 `Solid`，如果是，則會使用盡可能最少的三角形來填滿切割部分以覆蓋每個孔。

在以下範例中，從 `Mesh.BooleanDifference` 作業取得的空心網格被某個平面以某個角度切割。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
