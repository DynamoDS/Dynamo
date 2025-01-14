## 深入資訊
`Mesh.Sphere` 會建立一個中心位於輸入 `origin` 點，具有給定 `radius` 和 `divisions` 數的網格圓球。使用 `icosphere` 布林輸入在 `icosphere` 和 `UV-Sphere` 球形網格類型之間切換。與 UV 網格相比，icosphere 網格以更多規則三角形覆蓋圓球，在下游塑型作業中會提供更好的結果。UV 網格的兩極與圓球軸對齊，會繞著軸縱向產生三角形層。

在 icosphere 的情況下，繞著球體軸的三角形數可能會和指定劃分數一樣少，最多是該數的兩倍。`UV-sphere` 的劃分數決定繞著球體縱向產生的三角形層數。當 `divisions` 輸入設定為零時，節點會傳回一個預設 32 個劃分數的 UV 圓球 (適用於任一網格類型)。

在以下範例中，使用 `Mesh.Sphere` 節點建立兩個半徑和劃分數相同但使用不同方法的圓球。當 `icosphere` 輸入設定為 `True` 時，`Mesh.Sphere` 會傳回一個 `icosphere`。但是當 `icosphere` 輸入設定為 `False` 時，`Mesh.Sphere` 節點會傳回一個 `UV-sphere`。

## 範例檔案

![Example](./Autodesk.DesignScript.Geometry.Mesh.Sphere_img.jpg)
