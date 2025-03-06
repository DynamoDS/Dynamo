## 深入資訊
以下範例使用 `TSplineSurface.ToMesh` 節點將簡單的 T 雲形線方塊曲面轉換為網格。`minSegments` 輸入定義一個面在每個方向的最小線段數，這對於控制網格定義非常重要。`tolerance` 輸入透過增加更多頂點位置，以在給定公差內符合原始曲面來修正不準確度。結果是一個網格，使用 `Mesh.VertexPositions` 節點預覽定義。
輸出網格可以包含三角形和四邊形，在使用 MeshToolkit 節點時請謹記這一點。
___
## 範例檔案

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
