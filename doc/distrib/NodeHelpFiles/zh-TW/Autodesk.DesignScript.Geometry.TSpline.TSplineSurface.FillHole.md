## 深入資訊
以下範例使用 `TSplineSurface.FillHole` 節點填滿 T 雲形線圓柱曲面中的間隙，此節點需要以下輸入:
- `edges`: 從 T 雲形線曲面點選要填滿的多個邊界邊
- `fillMethod`: 0-3 的數值，表示填滿方法:
    * 0 使用鑲嵌方式填滿孔
    * 1 使用單一多邊形面填滿孔
    * 2 在孔中心建立一點，三角形面從該點向邊輻射
    * 3 與方法 2 類似，差異在於中心頂點會接合到一個頂點，而不是只堆疊在頂端。
- `keepSubdCreases`: 表示是否保留子分割縐摺的布林值。
___
## 範例檔案

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
