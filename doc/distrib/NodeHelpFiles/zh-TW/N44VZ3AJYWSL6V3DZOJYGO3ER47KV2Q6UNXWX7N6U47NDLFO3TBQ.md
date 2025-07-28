<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
頂點的功能價不僅僅只是相鄰邊的簡單計數，也會考慮影響頂點在其周圍區域混成的虛擬格線。它可讓您更仔細了解頂點及其邊在變形和細分作業期間如何影響曲面。
在規則頂點和 T 點上使用時，`TSplineVertex.FunctionalValence` 節點會傳回值「4」，表示曲面由網格形狀的雲形線引導。「4」以外的任何功能價表示頂點是 Y 點，圍繞頂點的混成將會變得較不平滑。

以下範例在 T 雲形線平面曲面的兩個 T 點頂點使用 `TSplineVertex.FunctionalValence`。`TSplineVertex.Valence` 節點傳回值 3，所選頂點的功能價為 4，是 T 點特有的值。使用 `TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Position` 節點顯示分析的頂點位置。

## 範例檔案

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
