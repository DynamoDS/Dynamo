## 深入資訊
`Mesh.CloseCracks` 會透過從網格物件移除內部邊界來封閉網格中的裂縫。網格塑型作業可能會自然產生內部邊界。如果移除退化邊，就可以在此作業中刪除三角形。以下範例對匯入的網格使用 `Mesh.CloseCracks`。使用 `Mesh.VertexNormals` 顯示重疊頂點。原始網格通過 Mesh.CloseCracks 後，邊數會減少，使用 `Mesh.EdgeCount` 節點比較邊數也會明顯看出這一點。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.CloseCracks_img.jpg)
