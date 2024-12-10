<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## 深入資訊
`Curve.ExtrudeAsSolid (curve, direction)` 使用輸入向量決定擠出方向，將輸入的封閉平面曲線擠出。使用向量長度作為擠出距離。此節點會將擠出的兩端覆蓋以建立實體。

在以下範例中，我們先使用 `NurbsCurve.ByPoints` 節點，以一組隨機產生的點作為輸入建立一條 NurbsCurve。使用 Code Block 指定 `Vector.ByCoordinates` 節點的 X、Y 和 Z 分量，然後使用此向量作為 `Curve.ExtrudeAsSolid` 的 direction 輸入。
___
## 範例檔案

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
