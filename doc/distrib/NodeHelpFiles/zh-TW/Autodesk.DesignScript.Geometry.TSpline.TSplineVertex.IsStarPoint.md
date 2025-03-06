## In-Depth
`TSplineVertex.IsStarPoint` 會傳回頂點是否為 Y 點。

當有 3 邊、5 邊或更多邊相接時，即會存在 Y 點。這種點會自然出現在方塊或四角球基本型中，最常在擠出 T 雲形線面、刪除面或執行合併時產生。與一般頂點和 T 點頂點不同，Y 點不受矩形控制點列控制。Y 點會讓其周圍的區域更難控制，並且可能產生失真情況，因此只在必要時才使用。Y 點不良的放置位置包括模型較銳利的零件 (例如縐摺邊)、曲率發生顯著變更的零件，或在開放曲面的邊上。

Y 點也決定 T 雲形線將如何轉換為邊界表現法 (BREP)。將 T 雲形線轉換為 BREP 時，它會在每個 Y 點處分割為單獨的曲面。

以下範例使用 `TSplineVertex.IsStarPoint` 查詢使用 `TSplineTopology.VertexByIndex` 選取的頂點是否為 Y 點。


## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
