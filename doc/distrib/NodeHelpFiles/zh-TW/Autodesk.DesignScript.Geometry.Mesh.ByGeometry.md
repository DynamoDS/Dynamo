## 深入資訊
`Mesh.ByGeometry` 會將 Dynamo 幾何圖形物件 (曲面或實體) 當作輸入，並轉換為網格。點和曲線沒有網格表現法，因此它們不是有效的輸入。轉換時產生的網格解析度由兩個輸入 `tolerance` 和 `maxGridLines` 控制。`tolerance` 設定網格與原始幾何圖形之間可接受的偏差，與網格大小有關。如果 `tolerance` 的值設定為 -1，Dynamo 會選擇合理的公差。`maxGridLines` 輸入設定 U 或 V 方向的最大格線數。格線數越多，有助於提高鑲嵌的平滑度。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)
