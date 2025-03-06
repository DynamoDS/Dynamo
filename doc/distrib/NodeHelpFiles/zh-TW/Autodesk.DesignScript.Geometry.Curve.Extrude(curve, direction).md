## 深入資訊
`Curve.Extrude (curve, direction)` 使用輸入向量決定擠出方向，將輸入曲線擠出。使用向量長度作為擠出距離。

在以下範例中，我們先使用 `NurbsCurve.ByControlPoints` 節點，以一組隨機產生的點作為輸入建立一條 NurbsCurve。使用 Code Block 指定 `Vector.ByCoordinates` 節點的 X、Y 和 Z 分量，然後使用此向量作為 `Curve.Extrude` 節點的 `direction` 輸入。
___
## 範例檔案

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
