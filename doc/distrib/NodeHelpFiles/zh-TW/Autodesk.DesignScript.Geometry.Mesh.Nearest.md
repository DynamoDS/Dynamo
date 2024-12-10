## 深入資訊
`Mesh.Nearest` 會傳回輸入網格上最接近給定點的點。傳回的點是將輸入點使用法線向量投影到通過該點的網格上，得到可能最接近的點。

以下範例建立一個簡單的使用案例顯示此節點的運作方式。輸入點在球形網格的上方，但不是就在網格上面。產生的點是位於網格上最接近的點。對照 `Mesh.Project` 節點 (使用相同的點和網格作為輸入，加上負 Z 方向的向量) 的輸出，產生的點是將輸入點直接往正下方投影到網格上。使用 `Line.ByStartAndEndPoint` 顯示將點投影到網格上的「軌跡」。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.Nearest_img.jpg)
