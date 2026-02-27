<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## 深入資訊
`Curve.SweepAsSurface` 會透過沿著指定路徑掃掠輸入曲線來建立曲面。在以下範例中，我們使用 Code Block 建立 `Arc.ByThreePoints` 節點的三點，建立要掃掠的曲線。建立的路徑曲線是沿著 x 軸的簡單直線。`Curve.SweepAsSurface` 會沿著建立曲面的路徑曲線移動輪廓曲線。`cutEndOff` 參數是布林值，用於控制掃掠曲面的端點處理方式。設定為 `true` 時，曲面端點會垂直 (法向) 於路徑曲線切割，產生乾淨、平直的終止面。設定為 `false` (預設) 時，曲面端點會跟著輪廓曲線的自然形狀而不做任何修剪，這可能會導致端點呈現斜角或不平整的情形 (取決於路徑曲率)。
___
## 範例檔案

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

