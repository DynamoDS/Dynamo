## 深入資訊
SelfIntersections 會傳回多邊形自身相交的所有點的清單。在以下範例中，我們先產生隨機未排序的角度和半徑清單，以用於 Points.ByCylindricalCoordinates。由於我們讓 elevation 保持常數，且未排序這些點的角度，因此使用 Polygon.ByPoints 建立的多邊形將在同一個平面且可能自身相交。然後，我們可以使用 SelfIntersections 找出交點
___
## 範例檔案

![SelfIntersections](./Autodesk.DesignScript.Geometry.Polygon.SelfIntersections_img.jpg)

