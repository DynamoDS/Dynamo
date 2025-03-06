<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## 深入資訊
`TSplineSurface.BridgeEdgesToEdges` 會將相同曲面或兩個不同曲面的兩組邊連接。節點需要的輸入如下所述。只需要前三個輸入就能產生橋樑，其餘輸入為選擇性。產生的曲面是第一組邊所屬曲面的子系。

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup`: 從相同 T 雲形線曲面或不同 T 雲形線曲面選取的邊。邊數必須相符，或為橋樑另一端邊數的倍數。
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


以下範例建立兩個 T 雲形線平面，並且使用 `TSplineSurface.DeleteEdges` 節點刪除每個平面中心的面。使用 `TSplineTopology.VertexByIndex` 節點收集刪除面周圍的邊。為了建立橋樑，使用兩組邊作為 `TSplineSurface.BrideEdgesToEdges` 節點的輸入，搭配其中一個曲面，就會建立橋樑。透過編輯 `spansCounts` 輸入，在橋樑中加入更多跨距。如果使用曲線作為 `followCurves` 的輸入，橋樑就會跟著提供曲線的方向。`keepSubdCreases`、`frameRotations`、`firstAlignVertices` 和 `secondAlignVertices` 輸入示範如何微調橋樑形狀。

## 範例檔案

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

