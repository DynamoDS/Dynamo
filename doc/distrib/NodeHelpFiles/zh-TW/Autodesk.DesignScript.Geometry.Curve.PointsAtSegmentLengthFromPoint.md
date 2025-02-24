## 深入資訊
PointsAtSegmentLengthFromPoint 會傳回沿著曲線指定點開始，根據輸入線段長度沿著曲線依序測量的點清單。在以下範例中，我們先使用 ByControlPoints 節點，以一組隨機產生的點作為輸入建立一條 Nurbs 曲線。使用 PointAtParameter 節點搭配設定為範圍 0 到 1 的數字滑棒，決定 PointsAtSegmentLengthFromPoint 節點沿曲線的初始點。最後，使用第二個數字滑棒調整要使用的曲線線段長度
___
## 範例檔案

![PointsAtSegmentLengthFromPoint](./Autodesk.DesignScript.Geometry.Curve.PointsAtSegmentLengthFromPoint_img.jpg)

