## In-Depth
`TSplineFace.Info` 會傳回 T 雲形線面的以下性質:
- `uvnFrame`: 關聯線上的點、U 向量、V 向量和 T 雲形線面的法線向量
- `index`: 面的索引
- `valence`: 形成面的頂點數或邊數
- `sides`: 每個 T 雲形線面的邊數

以下範例分別使用 `TSplineSurface.ByBoxCorners` 和 `TSplineTopology.RegularFaces` 建立 T 雲形線和選取其面。使用 `List.GetItemAtIndex` 點選 T 雲形線的特定面，使用 `TSplineFace.Info` 找出其性質。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
