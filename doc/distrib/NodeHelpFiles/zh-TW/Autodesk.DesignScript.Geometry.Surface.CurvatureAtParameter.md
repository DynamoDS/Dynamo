## 深入資訊
CurvatureAtParameter 使用 U 和 V 輸入參數，並傳回以曲面上 UV 位置的法線、U 方向和 V 方向為基礎的座標系統。法線向量決定 Z 軸，而 U 和 V 方向決定 X 和 Y 軸的方向。軸的長度由 U 和 V 曲率決定。在以下範例中，我們先使用 BySweep2Rails 建立一個曲面，然後使用兩個數字滑棒決定 U 和 V 參數，以使用 CurvatureAtParameter 節點建立座標系統。
___
## 範例檔案

![CurvatureAtParameter](./Autodesk.DesignScript.Geometry.Surface.CurvatureAtParameter_img.jpg)

