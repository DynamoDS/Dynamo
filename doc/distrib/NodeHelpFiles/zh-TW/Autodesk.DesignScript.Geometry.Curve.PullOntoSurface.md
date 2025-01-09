## 深入資訊
PullOntoSurface 會透過將輸入曲線投影到輸入曲面上，使用曲面的法線向量作為投影方向建立一條新曲線。在以下範例中，我們先使用 Surface.BySweep 節點建立曲面，此節點使用根據正弦曲線產生的曲線。使用此曲面在 PulOntoSurface 節點中作為要拉到上面的基準曲面。對於曲線，我們使用 Code Block 指定中心點的座標並使用數字滑棒控制半徑來建立圓。結果是圓到曲面上的投影。
___
## 範例檔案

![PullOntoSurface](./Autodesk.DesignScript.Geometry.Curve.PullOntoSurface_img.jpg)

