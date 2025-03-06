<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedFaces --->
<!--- VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA --->
## 深入資訊
以下範例使用 `TSplineTopology.DecomposedFaces` 節點檢驗具有擠出、細分和拉伸頂點和面的平面 T 雲形線曲面，此節點會傳回 T 雲形線曲面中包含的以下面類型清單:

- `all`: 所有面的清單
- `regular`: 規則面的清單
- `nGons`: 多邊形面的清單
- `border`: 邊界面的清單
- `inner`: 內側面的清單

使用 `TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Position` 節點亮顯曲面不同類型的面。
___
## 範例檔案

![TSplineTopology.DecomposedFaces](./VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA_img.gif)
