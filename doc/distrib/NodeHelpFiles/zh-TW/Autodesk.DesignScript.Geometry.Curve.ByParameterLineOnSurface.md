## 深入資訊
Curve.ByParameterLineOnSurface 沿兩個輸入 UV 座標之間的曲面建立一條直線。在以下範例中，我們先建立點格線，並沿 Z 方向平移某個隨機的量。使用 NurbsSurface.ByPoints 節點時會使用這些點來建立曲面。使用此曲面作為 ByParameterLineOnSurface 節點的 baseSurface。使用一組數字滑棒調整兩個 UV.ByCoordinates 節點的 U 和 V 輸入，然後決定直線在曲面上的起點和終點。
___
## 範例檔案

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

