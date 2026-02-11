## 深入資訊
Polygon.Center 透過計算角點的平均值來找出給定多邊形的中心。如果是凹多邊形，中心可能實際位於多邊形之外。在以下範例中，我們先產生隨機角度和半徑的清單，作為 Point.ByCylindricalCoordinates 的輸入。首先排序角度，確保產生的多邊形是按照從小到大的角度連接，因此不會自身相交。然後，我們可以使用 Center 得到點的平均值並找出多邊形中心。
___
## 範例檔案

![Center](./DSCore.String.Center_img.jpg)

