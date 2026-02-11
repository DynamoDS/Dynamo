## 深入資訊
ParameterAtPoint 會傳回曲線上指定點的參數值。如果輸入點不在曲線上，ParameterAtPoint 會傳回曲線上接近輸入點的點參數。在以下範例中，我們先使用 ByControlPoints 節點，以一組隨機產生的點作為輸入建立一條 Nurbs 曲線。使用 Code Block 額外建立一點以指定 x 和 y 座標。ParameterAtPoint 節點會傳回曲線上最接近輸入點的點處的參數。
___
## 範例檔案

![ParameterAtPoint](./Autodesk.DesignScript.Geometry.Curve.ParameterAtPoint_img.jpg)

