<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
此節點與 `TSplineSurface.UnweldEdges` 類似，會在一組頂點上執行取消接合作業。結果是，在所選頂點處接合的所有邊都會取消接合。與「取消縐摺」作業不同，取消縐摺會在頂點周圍產立銳利的轉變，但仍保持連接，「取消接合」則會產生不連續狀況。

以下範例使用 `TSplineSurface.UnweldVertices` 節點將 T 雲形線平面的其中一個所選頂點取消接合。所選頂點周圍的邊會產生不連續狀況，使用 `TSplineSurface.MoveVertices` 節點將頂點向上拉即可顯示。

## 範例檔案

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
