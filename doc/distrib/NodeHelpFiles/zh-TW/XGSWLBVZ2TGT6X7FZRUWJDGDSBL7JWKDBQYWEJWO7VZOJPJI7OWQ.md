## 深入資訊
`TSplineSurface.FlattenVertices(vertices, parallelPlane)` 透過將一組指定頂點與提供的 `parallelPlane` 輸入對齊，來改變控制點的位置。

以下範例使用 `TsplineTopology.VertexByIndex` 和 `TSplineSurface.MoveVertices` 節點位移 T 雲形線平面曲面的頂點，然後將曲面平移到旁邊方便預覽，並當作 `TSplineSurface.FlattenVertices(vertices, parallelPlane)` 節點的輸入。結果是所選頂點位於提供平面上的新曲面。
___
## 範例檔案

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
