## In-Depth
`TSplineVertex.Info` 會傳回 T 雲形線頂點的以下性質:
- `uvnFrame`: 關聯線上的點、U 向量、V 向量和 T 雲形線頂點的法線向量
- `index`: T雲形線曲面上所選頂點的索引
- `isStarPoint`: 選擇的頂點是否為 Y 點
- `isTpoint`: 選擇的頂點是否為 T 點
- `isManifold`: 選擇的頂點是否為流形
- `valence`: 所選 T 雲形線頂點上的邊數
- `functionalValence`: 頂點的功能價。請參閱 `TSplineVertex.FunctionalValence` 節點的文件以取得更多資訊。

以下範例使用 `TSplineSurface.ByBoxCorners` 和 `TSplineTopology.VertexByIndex` 分別建立 T 雲形線曲面並選取其頂點。使用 `TSplineVertex.Info` 收集有關所選頂點的上述資訊。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
