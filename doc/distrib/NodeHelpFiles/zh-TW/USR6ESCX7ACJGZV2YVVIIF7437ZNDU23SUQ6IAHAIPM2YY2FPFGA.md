## 深入資訊
`TSplineSurface.Thicken(vector, softEdges)` 會將指定向量引導的 T 雲形線曲面增厚。增厚作業會在 `vector` 方向複製曲面，然後透過接合曲面的邊來連接兩個曲面。`softEdges` 布林輸入控制產生的邊是平滑 (true) 或縐摺 (false)。

以下範例使用 `TSplineSurface.Thicken(vector, softEdges)` 節點將 T 雲形線擠出曲面增厚。產生的曲面會平移到旁邊方便檢視。


___
## 範例檔案

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)
