## In-Depth

以下範例在 T 雲形線曲面的一列邊執行取消接合作業。結果是，所選邊的頂點會不連貫。與「取消縐摺」不同，取消縐褶會在邊周圍產生銳利的轉變，但仍然保持連接，「取消接合」則會產生不連續狀況。透過比較作業執行前後的頂點數可以證實。後續對取消接合的邊或頂點進行任何作業，也會顯示出曲面沿取消接合的邊不連續。

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldEdges_img.jpg)
