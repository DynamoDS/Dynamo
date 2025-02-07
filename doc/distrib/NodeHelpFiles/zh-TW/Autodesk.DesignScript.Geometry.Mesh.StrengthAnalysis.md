## 深入資訊
 `Mesh.StrengthAnalysis` 節點會傳回每個頂點的代表顏色清單。此結果可與 `Mesh.ByMeshColor` 節點一起使用。網格中較強的區域顯示為綠色，較弱的區域以黃色到紅色的熱度圖表示。如果網格太粗糙或不規則 (亦即有許多細長三角形)，分析可能會產生假警報。您可以先嘗試使用 `Mesh.Remesh` 產生規則的網格，再對該網格呼叫 `Mesh.StrengthAnalysis` 產生比較好的結果。

以下範例使用 `Mesh.StrengthAnalysis` 對格線形狀的網格結構強度標示顏色。結果是與網格頂點長度相符的顏色清單。此清單可與 `Mesh.ByMeshColor` 節點一起使用為網格上色。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
