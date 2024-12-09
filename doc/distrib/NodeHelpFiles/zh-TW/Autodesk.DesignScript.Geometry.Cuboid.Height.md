## 深入資訊
`Cuboid.Height` 會傳回輸入立方體的高度。請注意，如果立方體已轉換為具有某個比例係數的其他座標系統，則會傳回立方體的原始尺寸，而非世界空間尺寸。換言之，如果您建立寬度 (X 軸) 為 10 的立方體，並轉換為沿 X 放大 2 倍的 CoordinateSystem，則寬度仍會是 10。

在以下範例中，我們透過角點產生立方體，然後使用 `Cuboid.Height` 節點找出其高度。

___
## 範例檔案

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

