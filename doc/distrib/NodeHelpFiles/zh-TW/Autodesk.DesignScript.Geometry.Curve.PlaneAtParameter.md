## 深入資訊
PlaneAtParameter 會傳回一個以曲線指定參數處校準的平面。產生的平面的法線向量將對應曲線的切線。曲線的參數化範圍從 0 到 1，0 表示曲線起點，1 表示曲線終點。在以下範例中，我們先使用 ByControlPoints 節點，以一組隨機產生的點作為輸入建立一條 Nurbs 曲線。使用設定為範圍 0 到 1 的數字滑棒控制 PlaneAtParameter 節點的參數輸入。
___
## 範例檔案

![PlaneAtParameter](./Autodesk.DesignScript.Geometry.Curve.PlaneAtParameter_img.jpg)

