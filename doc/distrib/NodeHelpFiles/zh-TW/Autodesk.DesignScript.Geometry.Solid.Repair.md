## 深入資訊
`Solid.Repair` 會嘗試修復具有無效幾何圖形的實體，可能同時執行最佳化。`Solid.Repair` 節點會傳回新的實體物件。

當您對匯入或轉換的幾何圖形執行作業卻發生錯誤時，此節點非常有用。

在以下範例中，使用 `Solid.Repair` 修復 **.SAT** 檔案中的幾何圖形。檔案中的幾何圖形無法進行布林運算或修剪，`Solid.Repair` 會清理導致失敗的任何*無效幾何圖形*。

您通常不需要對您在 Dynamo 中建立的幾何圖形使用此功能，只需對外部來源的幾何圖形使用。如果情況並非如此，請向 Dynamo 的 Github 團隊回報錯誤
___
## 範例檔案

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
