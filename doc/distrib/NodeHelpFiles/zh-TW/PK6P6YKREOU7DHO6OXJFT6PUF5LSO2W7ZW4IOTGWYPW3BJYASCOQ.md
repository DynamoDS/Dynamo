<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial` 定義 T 雲形線幾何圖形是否有徑向對稱。只有允許徑向對稱的 T 雲形線基本型 - 圓錐、圓球、迴轉、圓環，才能引入徑向對稱。建立 T 雲形線幾何圖形時如果建立了徑向對稱，則會影響所有後續的作業和變更。

必須定義所需的 `symmetricFaces` 數才能套用對稱，1 為最小值。無論 T 雲形線曲面一開始必須有多少個半徑跨距和高度跨距，都會進一步分割為選擇的 `symmetricFaces` 數。

以下範例建立 `TSplineSurface.ByConePointsRadii` 並使用 `TSplineInitialSymmetry.ByRadial` 節點套用徑向對稱。然後使用 `TSplineTopology.RegularFaces` 和 `TSplineSurface.ExtrudeFaces` 節點分別選取和擠出 T 雲形線曲面的面。擠出會對稱套用，對稱面數的滑棒示範如何細分徑向跨距。

## 範例檔案

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
