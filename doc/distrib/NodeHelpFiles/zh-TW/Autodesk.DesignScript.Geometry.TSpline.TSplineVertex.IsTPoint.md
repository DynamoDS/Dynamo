## In-Depth
`TSplineVertex.IsTPoint` 會傳回頂點是否為 T 點。T 點是控制點部分列末端的頂點。

以下範例對 T 雲形線方塊基本型使用 `TSplineSurface.SubdivideFaces`，說明在曲面中加入 T 點的其中一種方式。使用 `TSplineVertex.IsTPoint` 節點確認索引處的頂點是 T 點。為了更清楚顯示 T 點的位置，使用 `TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Position` 節點。



## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
