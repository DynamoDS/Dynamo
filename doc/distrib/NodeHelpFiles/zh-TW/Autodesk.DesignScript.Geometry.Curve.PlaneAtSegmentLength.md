## 深入資訊
PlaneAtSegmentLength 會傳回一個以曲線在起點開始沿著曲線測量指定距離的點處校準的平面。如果輸入長度大於曲線的總長度，此節點將使用曲線的終點。產生的平面的法線向量將對應曲線的切線。在以下範例中，我們先使用 ByControlPoints 節點，以一組隨機產生的點作為輸入建立一條 Nurbs 曲線。使用數字滑棒控制 PlaneAtSegmentLength 節點的參數輸入。
___
## 範例檔案

![PlaneAtSegmentLength](./Autodesk.DesignScript.Geometry.Curve.PlaneAtSegmentLength_img.jpg)

