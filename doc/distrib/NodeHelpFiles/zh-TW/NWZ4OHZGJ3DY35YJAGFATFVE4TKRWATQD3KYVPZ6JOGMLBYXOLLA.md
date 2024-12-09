<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## 深入資訊
`Curve.ExtrudeAsSolid (curve, distance)` 使用輸入數字決定擠出距離，將輸入的封閉平面曲線擠出。擠出方向由曲線所在平面的法線向量決定。此節點會將擠出的兩端覆蓋以建立實體。

在以下範例中，我們先使用 `NurbsCurve.ByPoints` 節點，以一組隨機產生的點作為輸入建立一條 NurbsCurve，然後使用 `Curve.ExtrudeAsSolid` 節點將曲線擠出為實體。使用數字滑棒作為 `Curve.ExtrudeAsSolid` 節點的 `distance` 輸入。
___
## 範例檔案

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
