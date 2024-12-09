<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
`TSplineSurface.ByPlaneBestFitThroughPoints` 會使用點清單產生 T 雲形線基本型平面曲面。若要建立 T 雲形線平面，節點使用以下輸入:
- `points`: 一組定義平面方位和原點的點。如果輸入點不在單一平面上，則會根據最佳擬合的結果決定平面的方位。至少需要三個點才能建立曲面。
- `minCorner` 和 `maxCorner`: 平面的角點，以 X 和 Y 值表示點 (將忽略 Z 座標)。如果將輸出 T 雲形線曲面平移到 XY 平面上，這些角點表示輸出 T 雲形線曲面的範圍。`minCorner` 和 `maxCorner` 點在 3D 不必與角落頂點重合。
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

以下範例使用隨機產生的點清單建立 T 雲形線平面曲面。曲面的大小由 `minCorner` 和 `maxCorner` 輸入的兩點控制。

## 範例檔案

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
