## 深入資訊
`Curve.Extrude (curve, distance)` 使用輸入數字決定擠出距離，將輸入曲線擠出。沿著曲線使用法線向量方向作為擠出方向。

在以下範例中，我們先使用 `NurbsCurve.ByControlPoints` 節點，以一組隨機產生的點作為輸入建立一條 NurbsCurve，然後使用 `Curve.Extrude` 節點將曲線擠出。使用數字滑棒作為 `Curve.Extrude` 節點的 `distance` 輸入。
___
## 範例檔案

![Curve.Extrude(curve, distance)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20distance)_img.jpg)
