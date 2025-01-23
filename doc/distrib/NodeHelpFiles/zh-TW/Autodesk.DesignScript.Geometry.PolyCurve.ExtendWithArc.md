## 深入資訊
ExtendWithArc 會在輸入 PolyCurve 的起點或終點加上一條圓弧，並傳回一條結合的 PolyCurve。radius 輸入決定圓的半徑，length 輸入決定弧沿著圓的距離。總長度必須小於或等於給定半徑的一個完整圓的長度。產生的弧將與輸入 PolyCurve 的終點相切。endOrStart 的布林輸入控制 PolyCurve 哪一端要建立弧。值 true 會在 PolyCurve 的終點建立弧，值 false 會在 PolyCurve 的起點建立弧。在以下範例中，我們先使用一組隨機點和 PolyCurve.ByPoints 產生一條 PolyCurve，然後使用兩個數字滑棒和一個布林切換設定 ExtendWithArc 的參數。
___
## 範例檔案

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

