<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## 深入資訊
以下範例使用 `TSplineTopology.DecomposedVertices` 節點檢驗具有擠出、細分和拉伸頂點和面的平面 T 雲形線曲面，此節點會傳回 T 雲形線曲面中包含的以下頂點類型清單:

- `all`: 所有頂點的清單
- `regular`: 規則頂點的清單
- `tPoints`: T 點頂點的清單
- `starPoints`: Y 點頂點的清單
- `nonManifold`: 非流形頂點的清單
- `border`: 邊界頂點的清單
- `inner`: 內側頂點的清單

使用 `TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Position` 節點亮顯曲面不同類型的頂點。

___
## 範例檔案

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
