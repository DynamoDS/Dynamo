## 深入資訊
Extend 會將輸入曲線延伸給定的輸入距離。pickSide 輸入以曲線的起點或終點當作輸入，並決定要延伸曲線的哪一端。在以下範例中，我們先使用 ByControlPoints 節點，以一組隨機產生的點作為輸入建立一條 Nurbs 曲線。我們使用查詢節點 Curve.EndPoint 找出曲線的終點來作為「pickSide」輸入。數字滑棒可讓我們控制延伸的距離。
___
## 範例檔案

![Extend](./Autodesk.DesignScript.Geometry.Curve.Extend_img.jpg)

