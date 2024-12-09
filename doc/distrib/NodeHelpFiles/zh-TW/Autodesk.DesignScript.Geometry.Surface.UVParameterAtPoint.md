## 深入資訊
UVParameterAtPoint 會找出曲面上輸入點處曲面的 UV 位置。如果輸入點不在曲面上，此節點將找出曲面上最接近輸入點的點。在以下範例中，我們先使用 BySweep2Rails 建立一個曲面，然後使用 Code Block 指定要找出 UV 參數的點。因為該點不在曲面上，所以節點會使用曲面上最接近的點作為找出 UV 參數的位置。
___
## 範例檔案

![UVParameterAtPoint](./Autodesk.DesignScript.Geometry.Surface.UVParameterAtPoint_img.jpg)

