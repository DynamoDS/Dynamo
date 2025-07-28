<!--- Autodesk.DesignScript.Geometry.Curve.Extrude(curve, direction, distance) --->
<!--- 5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA --->
## 深入資訊
`Curve.Extrude (curve, direction, distance)` 使用輸入向量決定擠出方向，將輸入曲線擠出。使用單獨的 `distance` 輸入作為擠出距離。

在以下範例中，我們先使用 `NurbsCurve.ByControlPoints` 節點建立 NurbsCurve，並使用一組隨機產生的點作為輸入。使用 Code Block 指定 `Vector.ByCoordinates` 節點的 X、Y 和 Z 分量，然後使用此向量作為 `Curve.Extrude` 節點的 `direction` 輸入，同時使用 `number slider` 控制 `distance` 輸入。
___
## 範例檔案

![Curve.Extrude(curve, direction, distance)](./5NB3FDYBJDTGURCB4X7W2I7P2TIGXAXPEUVWUMM2BTWHJ3GXRJQA_img.jpg)
