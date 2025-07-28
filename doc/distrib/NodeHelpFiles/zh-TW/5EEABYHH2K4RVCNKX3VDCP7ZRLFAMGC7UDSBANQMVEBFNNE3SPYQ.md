<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## 深入資訊
`Curve.NormalAtParameter (curve, param)` 會傳回一個以曲線指定參數處之法線方向校準的向量。曲線的參數化範圍從 0 到 1，0 表示曲線起點，1 表示曲線終點。

在以下範例中，我們先使用 `NurbsCurve.ByControlPoints` 節點，以一組隨機產生的點作為輸入建立一條 NurbsCurve。使用設定為範圍 0 到 1 的數字滑棒控制 `Curve.NormalAtParameter` 節點的 `parameter` 輸入。
___
## 範例檔案

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
