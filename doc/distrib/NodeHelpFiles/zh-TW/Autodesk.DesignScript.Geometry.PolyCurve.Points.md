## 深入資訊
`PolyCurve.Points` 會傳回第一條組成曲線的起點，以及所有其他組成曲線的終點。如果是封閉的 PolyCurve，不會傳回重複點。

以下範例將 `Polygon.RegularPolygon` 分解成一個曲線清單，然後重新接合成一條 PolyCurve。接著使用 `PolyCurve.Points` 傳回 PolyCurve 的點。
___
## 範例檔案

![PolyCurve.Points](./Autodesk.DesignScript.Geometry.PolyCurve.Points_img.jpg)
