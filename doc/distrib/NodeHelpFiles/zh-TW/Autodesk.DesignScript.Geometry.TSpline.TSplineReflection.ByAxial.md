## In-Depth
`TSplineReflection.ByAxial` 會傳回 `TSplineReflection` 物件，此物件可作為 `TSplineSurface.AddReflections` 節點的輸入。
`TSplineReflection.ByAxial` 節點的輸入是作為鏡射平面的平面。與 TSplineInitialSymmetry 非常類似，一旦為 TSplineSurface 建立了 TSplineReflection，就會影響所有後續的作業和變更。

以下範例使用 `TSplineReflection.ByAxial` 在 T 雲形線圓錐頂部建立 TSplineReflection，然後使用該反射作為 `TSplineSurface.AddReflections` 節點的輸入，反射圓錐並傳回新的 T 雲形線曲面。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
