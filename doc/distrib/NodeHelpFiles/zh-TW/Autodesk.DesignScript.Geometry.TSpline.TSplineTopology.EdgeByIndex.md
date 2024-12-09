## 深入資訊
以下範例使用 `TSplineSurface.ByBoxLengths` 節點建立具有指定原點、寬度、長度、高度、跨距和對稱的 T 雲形線方塊。
然後使用 `EdgeByIndex` 從產生曲面的邊清單中選取邊，再使用 `TSplineSurface.SlideEdges`，讓選取的邊沿相鄰邊滑動，後面接著其對稱的對應邊。
___
## 範例檔案

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
