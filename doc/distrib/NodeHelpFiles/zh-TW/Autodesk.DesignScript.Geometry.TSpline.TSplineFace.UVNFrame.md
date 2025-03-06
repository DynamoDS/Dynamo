## In-Depth
面的 UVNFrame 透過傳回法線向量和 UV 方向，提供有關面位置和方位的有用資訊。
以下範例使用 `TSplineFace.UVNFrame` 節點顯示四角球基本型上的面分佈。使用 `TSplineTopology.DecomposedFaces` 查詢所有面，然後使用 `TSplineFace.UVNFrame` 節點擷取面形心的位置作為點。使用 `TSplineUVNFrame.Position` 節點顯示這些點。在節點的右鍵功能表中啟用「展示標籤」，會在背景預覽中顯示標籤。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)
