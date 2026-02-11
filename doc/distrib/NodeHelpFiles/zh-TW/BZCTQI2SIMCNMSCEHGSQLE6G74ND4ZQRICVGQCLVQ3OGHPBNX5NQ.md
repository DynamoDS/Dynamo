<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## 深入資訊
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` 先在輸入參數清單決定的點處分割曲線，然後由 `discardEvenSegments` 輸入的布林值決定，要傳回奇數編號線段或偶數編號線段。

在以下範例中，我們先使用 `NurbsCurve.ByControlPoints` 節點，以一組隨機產生的點作為輸入建立一條 NurbsCurve。使用 `code block` 建立一個 0 到 1 之間且步長為 0.1 的數字範圍，用來作為 `Curve.TrimSegmentsByParameter` 節點的輸入參數，產生一個相當於原始曲線虛線版本的曲線清單。
___
## 範例檔案

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
