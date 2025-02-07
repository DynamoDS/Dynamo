## 深入資訊
`Mesh.EdgeCount` 會建立一個底部中心位於輸入原點，具有底部半徑和頂部半徑、高度和 `divisions` 數等輸入值的網格圓錐。`divisions` 數與圓錐頂部和底部建立的頂點數對應。如果 `divisions` 數為 0，Dynamo 會使用預設值。沿 Z 軸的劃分數永遠等於 5。`cap` 輸入使用 `Boolean` 控制圓錐頂部是否封閉。
在以下範例中，使用 `Mesh.Cone` 節點建立一個有 6 個劃分數的圓錐形網格，因此圓錐的底部和頂部是六邊形。使用 `Mesh.Triangles` 節點顯示網格三角形的分佈。


## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
