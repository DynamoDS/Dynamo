## 深入資訊
Solid.ByJoinedSurfaces 以曲面清單當作輸入，並傳回由曲面定義的單一實體。曲面必須定義一個封閉曲面。在以下範例中，我們先以圓作為基底幾何圖形。修補圓以建立曲面，在 Z 方向平移該曲面，然後擠出圓以產生邊。使用 List.Create 建立由基底曲面、側邊曲面和上方曲面組成的清單，然後使用 ByJoinedSurfaces 將清單轉換為單一封閉實體。
___
## 範例檔案

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

