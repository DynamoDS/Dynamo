## 深入資訊
以下範例使用 `TSplineTopology.EdgeByIndex` 節點建立簡單的 T 雲形線方塊曲面，並選取其中一條邊。若要更清楚得知所選頂點的位置，可以透過 `TSplineEdge.UVNFrame` 和 `TSplineUVNFrame.Position` 節點顯示該頂點。將選擇的邊及其所屬的曲面作為 `TSplineSurface.SlideEdges` 節點的輸入傳入。`amount` 輸入決定邊向其相鄰邊滑動的程度，以百分比表示。`roundness` 輸入控制斜切的平坦度或圓度。在方塊模式中更能了解圓度的效果。滑動作業的結果會平移到旁邊進行預覽。

___
## 範例檔案

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
