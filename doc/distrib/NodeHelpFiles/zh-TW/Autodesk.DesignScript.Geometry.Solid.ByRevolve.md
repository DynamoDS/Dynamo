## 深入資訊
`Solid.ByRevolve` 透過將給定的輪廓曲線繞軸旋轉來建立一個曲面。軸由 `axisOrigin` 點和 `axisDirection` 向量定義。起始角度決定曲面的起始位置 (以度為單位)，`sweepAngle` 決定曲面繼續繞軸的距離。

在以下範例中，我們使用透過餘弦函數產生的曲線作為輪廓曲線，使用兩個數字滑棒控制 `startAngle` 和 `sweepAngle`。在此範例中，`axisOrigin` 和 `axisDirection` 則分別使用世界原點和世界 z 軸的預設值。

___
## 範例檔案

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

