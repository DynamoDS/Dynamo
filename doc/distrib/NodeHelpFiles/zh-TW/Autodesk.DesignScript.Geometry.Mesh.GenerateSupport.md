## 深入資訊
使用 `Mesh.GenerateSupport` 節點為輸入網格幾何圖形增加支撐，以準備進行 3D 列印。要成功列印帶有突出物的幾何圖形必須要有支撐，才能確保有適當的圖層附著，並防止材料在列印過程中下垂。`Mesh.GenerateSupport` 會偵測突出物並自動產生樹狀支撐，這些支撐消耗的材料很少，與列印表面的接觸很少，也比較容易移除。如果沒有偵測到任何突出物，`Mesh.GenerateSupport` 節點的結果會是相同的網格，旋轉變成最適合列印方位並平移到 XY 平面。支撐的規劃由下列輸入控制:
- baseHeight - 定義支撐最低部分 (其底部) 的厚度
- baseDiameter 控制支撐底部的大小
- postDiameter 輸入控制每個支撐在其中間的大小
- tipHeight 和 tipDiameter 控制支撐在尖端處與列印表面接觸的大小
以下範例使用 `Mesh.GenerateSupport` 節點在網格中增加字母 T 形的支撐。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.GenerateSupport_img.jpg)
