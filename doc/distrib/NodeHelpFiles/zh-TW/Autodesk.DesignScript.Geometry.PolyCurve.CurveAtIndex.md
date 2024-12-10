## 深入資訊
CurveAtIndex 會傳回給定 PolyCurve 在輸入索引處的曲線段。如果 PolyCurve 中的曲線數小於給定索引，CurveAtIndex 將傳回空值。endOrStart 輸入接受布林值 true 或 false。如果為 true，CurveAtIndex 會從 PolyCurve 的第一段開始計數。如果為 false，則會從最後一段向後計數。在以下範例中，我們產生一組隨機點，然後使用 PolyCurve.ByPoints 建立一條開放的 PolyCurve。然後，我們可以使用 CurveAtIndex 從 PolyCurve 擷取特定的線段。
___
## 範例檔案

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

