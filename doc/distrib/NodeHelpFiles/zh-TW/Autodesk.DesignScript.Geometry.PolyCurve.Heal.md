## 深入資訊
`PolyCurve.Heal` 以自身相交的 PolyCurve 當作輸入，傳回不自身相交的新 PolyCurve。輸入 PolyCurve 不能有 3 個以上的自身相交。換言之，如果 PolyCurve 有任何單一線段與其他 2 個以上的線段相交，癒合將無法運作。如果輸入大於 0 的 `trimLength`，則不會修剪長度超過 `trimLength` 的結束線段。

在以下範例中，使用 `PolyCurve.Heal` 癒合自身相交的 PolyCurve。
___
## 範例檔案

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
