## 深入資訊
標準 T 雲形線曲面的條件是所有 T 點與 Y 點之間至少分隔兩條等角曲線。將 T 雲形線曲面轉換為 NURBS 曲面時，需要標準化。

在以下範例中，透過 `TSplineSurface.ByBoxLengths` 產生的 T 雲形線曲面會細分其中一面。使用 `TSplineSurface.IsStandard` 檢查曲面是否標準，但會產生否定結果。
然後使用 `TSplineSurface.Standardize` 標準化曲面。引入新控制點而不改變曲面的形狀。使用 `TSplineSurface.IsStandard` 檢查產生的曲面，確認其現在是標準曲面。
使用 `TSplineFace.UVNFrame` 和 `TSplineUVNFrame.Position` 節點亮顯曲面中細分的面。
___
## 範例檔案

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
