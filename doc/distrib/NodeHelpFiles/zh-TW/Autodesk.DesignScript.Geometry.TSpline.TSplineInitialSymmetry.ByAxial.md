## 深入資訊
`TSplineInitialSymmetry.ByAxial` 定義 T 雲形線幾何圖形是否沿選擇的軸 (x, y, z) 對稱。對稱可在一個、兩個或所有三個軸產生。建立 T 雲形線幾何圖形時如果建立了對稱性，則會影響所有後續的作業和變更。

以下範例使用 `TSplineSurface.ByBoxCorners` 建立 T 雲形線曲面。在此節點的輸入中，使用 `TSplineInitialSymmetry.ByAxial` 定義曲面的初始對稱，然後使用 `TSplineTopology.RegularFaces` 和 `TSplineSurface.ExtrudeFaces` 分別選取和擠出 T 雲形線曲面的面，最後對使用 `TSplineInitialSymmetry.ByAxial` 節點定義的對稱軸將擠出作業鏡射。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
