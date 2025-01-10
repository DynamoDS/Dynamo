## 深入資訊
Surface.ByRuledLoft 會將曲線的有序清單作為輸入，並將這些曲線之間斷面混成一個直條紋的曲面。與 ByLoft 相比，ByRuledLoft 可能稍快一些，但產生的曲面較不平滑。在以下範例中，我們從沿 X 軸的線開始。我們將這條線沿 Y 方向平移成一系列隨正弦曲線起伏的線。使用這個產生的線清單作為 Surface.ByRuledLoft 的輸入，會產生一個在輸入曲線之間有直線段的曲面。
___
## 範例檔案

![ByRuledLoft](./Autodesk.DesignScript.Geometry.Surface.ByRuledLoft_img.jpg)

