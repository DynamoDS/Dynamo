## 深入資訊
Curve.ByIsoCurveOnSurface 透過指定 U 或 V 方向，並指定相反方向建立曲線的參數，來建立曲面上的等角曲線。「direction」輸入決定要建立等角曲線的方向。值 1 對應 U 方向，值 0 對應 V 方向。在以下範例中，我們先建立點格線，並沿 Z 方向平移某個隨機的量。使用 NurbsSurface.ByPoints 節點時會使用這些點來建立曲面。使用此曲面作為 ByIsoCurveOnSurface 節點的 baseSurface。使用設定為 0 到 1 的範圍和步數 1 的數字滑棒控制我們要沿 u 方向還是 v 方向擷取等角曲線。使用第二個數字滑棒決定以哪個參數擷取等角曲線。
___
## 範例檔案

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

