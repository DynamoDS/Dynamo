## 深入資訊
當您需要將封閉的 NURBS 曲線匯出至其他系統 (例如 Alias)，或當該系統需要週期形式的曲線時，請使用 `NurbsCurve.PeriodicControlPoints`。許多 CAD 工具需要此形式才能達到雙向準確度。

`PeriodicControlPoints` 會以*週期*形式傳回控制點。`ControlPoints` 會以*鎖模*形式傳回這些點。兩個陣列的點數相同；這是兩種描述同一條曲線的方式。在週期形式中，最後幾個控制點與前幾個控制點相符 (與曲線次數一樣多)，因此曲線會平滑封閉。鎖模形式使用不同的配置，因此兩個陣列中的點位置不同。

以下範例使用 `NurbsCurve.ByControlPointsWeightsKnots` 建置一條週期性 NURBS 曲線。Watch 節點會比較 `ControlPoints` 和 `PeriodicControlPoints`，因此您可以看到相同長度但不同的點位置。ControlPoints 顯示為紅色，因此與背景預覽中的 PeriodicControlPoints (黑色) 明顯不同。
___
## 範例檔案

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
