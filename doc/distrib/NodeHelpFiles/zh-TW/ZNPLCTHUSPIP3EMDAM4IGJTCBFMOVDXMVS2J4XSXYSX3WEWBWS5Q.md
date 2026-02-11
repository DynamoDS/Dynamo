<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## 深入資訊
CoordinateSystemAtSegmentLength 會傳回一個以輸入曲線在起點開始測量指定曲線長度處校準的座標系統。產生的座標系統，其 X 軸是曲線的法線方向，Y 軸是曲線在指定長度處的切線方向。在以下範例中，我們先使用 ByControlPoints 節點，以一組隨機產生的點作為輸入建立一條 Nurbs 曲線。使用數字滑棒控制 CoordinateSystemAtParameter 節點的線段長度輸入。如果指定的長度比曲線的長度長，此節點會傳回曲線終點處的座標系統。
___
## 範例檔案

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

