## 深入資訊
SplitByPoints 會在指定的點分割輸入曲線，並傳回產生的線段清單。如果指定的點不在曲線上，此節點將沿曲線尋找最接近輸入點的點，並在這些產生的點分割曲線。在以下範例中，我們先使用 ByPoints 節點，以一組隨機產生的點作為輸入建立一條 Nurbs 曲線。在 SplitByPoints 節點中使用同一組點作為點清單。結果是產生的點之間的曲線線段清單。
___
## 範例檔案

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

