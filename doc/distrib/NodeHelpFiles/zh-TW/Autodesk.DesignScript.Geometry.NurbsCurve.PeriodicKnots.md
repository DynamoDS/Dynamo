## 深入資訊
當您需要將封閉的 NURBS 曲線匯出至其他系統 (例如 Alias)，或當該系統需要週期形式的曲線時，請使用 `NurbsCurve.PeriodicKnots`。許多 CAD 工具需要此形式才能達到雙向準確度。

`PeriodicKnots` 會以*週期* (未鎖模) 形式傳回節點向量。`Knots` 會以*鎖模*形式傳回向量。兩個陣列的長度相同；這是兩種描述同一條曲線的方式。在鎖模形式中，節點在起點和終點處重複出現，因此曲線被限制在參數範圍內。在週期形式中，節點間距在起點和終點處重複出現，因此形成一個平滑的封閉迴路。

以下範例使用 `NurbsCurve.ByControlPointsWeightsKnots` 建置一條週期性 NURBS 曲線。Watch 節點會比較 `Knots` 和 `PeriodicKnots`，因此您可以看到相同長度但不同值。Knots 是鎖模形式 (在終點處重複出現節點)，PeriodicKnots 是未鎖模形式，重複出現定義曲線週期性的差異樣式。
___
## 範例檔案

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
