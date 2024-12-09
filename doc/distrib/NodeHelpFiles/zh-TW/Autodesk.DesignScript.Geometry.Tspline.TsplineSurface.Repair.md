## 深入資訊
在以下範例中，T 雲形線曲面變成無效，透過在背景預覽中看到重疊面即可觀察到此情況。使用 `TSplineSurface.EnableSmoothMode` 節點啟用平滑模式失敗可確認該曲面無效。另一個線索是，即使曲面一開始就啟用了平滑模式，`TSplineSurface.IsInBoxMode` 節點仍傳回 `true`。

若要修復曲面，請讓它通過 `TSplineSurface.Repair` 節點。結果是有效的曲面，成功啟用平滑預覽模式可確認此結果。
___
## 範例檔案

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
