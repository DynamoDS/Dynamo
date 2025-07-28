<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormal` 使用原點和法線向量產生 T 雲形線基本型平面曲面。若要建立 T 雲形線平面，節點使用以下輸入:
- `origin`: 定義平面原點的點。
- `normal`: 指定所建立平面法線方向的向量。
- `minCorner` 和 `maxCorner`: 平面的角點，以 X 和 Y 值表示點 (將忽略 Z 座標)。如果將輸出 T 雲形線曲面平移到 XY 平面上，這些角點表示輸出 T 雲形線曲面的範圍。`minCorner` 和 `maxCorner` 點在 3D 不必與角落頂點重合。例如，當 `minCorner` 設定為 (0,0) 而 `maxCorner` 為 (5,10) 時，平面寬度和長度將分別為 5 和 10。
- `xSpans` 和 `ySpans`: 寬度和長度跨距數/平面的劃分數
- `symmetry`: 幾何圖形是否相對於其 X、Y 和 Z 軸對稱
- `inSmoothMode`: 產生的幾何圖形將以平滑模式還是方塊模式顯示

以下範例使用提供的原點和法線 (X 軸的向量) 建立 T 雲形線平面曲面。曲面的大小由 `minCorner` 和 `maxCorner` 輸入的兩點控制。

## 範例檔案

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
