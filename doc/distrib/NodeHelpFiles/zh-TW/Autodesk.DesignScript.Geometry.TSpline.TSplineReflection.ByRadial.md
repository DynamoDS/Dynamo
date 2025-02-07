## In-Depth
`TSplineReflection.ByRadial` 會傳回 `TSplineReflection` 物件，此物件可作為 `TSplineSurface.AddReflections` 節點的輸入。節點以平面當作輸入，將平面的法線當作旋轉幾何圖形的軸。與 TSplineInitialSymmetry 非常類似，在建立 TSplineSurface 時一旦建立了 TSplineReflection，就會影響所有後續的作業和變更。

以下範例使用 `TSplineReflection.ByRadial` 定義 T 雲形線曲面的反射。使用 `segmentsCount` 和 `segmentAngle` 輸入控制幾何圖形沿給定平面法線反射的方式，然後使用節點的輸出作為 `TSplineSurface.AddReflections` 節點的輸入，建立新的 T 雲形線曲面。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
