<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## 深入資訊
`Curve.SweepAsSolid` 會透過沿著指定路徑掃掠輸入封閉輪廓曲線來建立實體。

在以下範例中，我們使用一個矩形作為基礎輪廓曲線。使用餘弦函數搭配一系列角度改變一組點的 X 座標來建立路徑。使用這些點作為 `NurbsCurve.ByPoints` 節點的輸入，然後使用 `Curve.SweepAsSolid` 節點沿著建立的餘弦曲線掃掠矩形來建立實體。
___
## 範例檔案

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
