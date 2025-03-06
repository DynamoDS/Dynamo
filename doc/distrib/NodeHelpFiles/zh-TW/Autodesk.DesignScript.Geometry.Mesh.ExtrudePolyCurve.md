## 深入資訊
`Mesh.ExtrudePolyCurve` 節點會將提供的 `polycurve` 沿指定的向量方向擠出 `height` 輸入設定的給定距離。如果是開放的 polycurve，會連接第一點到最後一點加以封閉。如果初始 `polycurve` 是平面的且不自身相交，則產生的網格可以選擇加蓋以形成實體網格。
以下範例使用 `Mesh.ExtrudePolyCurve` 根據封閉的 PolyCurve 建立封閉網格。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.ExtrudePolyCurve_img.jpg)
