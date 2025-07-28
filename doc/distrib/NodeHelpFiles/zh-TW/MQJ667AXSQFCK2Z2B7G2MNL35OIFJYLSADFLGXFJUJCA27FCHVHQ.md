<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## 深入資訊
`TSplineSurface.BridgeEdgesToFaces` 會將相同曲面或兩個不同曲面的兩組面連接。節點需要的輸入如下所述。只需要前三個輸入就能產生橋樑，其餘輸入為選擇性。產生的曲面是第一組邊所屬曲面的子系。

以下範例使用 `TSplineSurface.ByTorusCenterRadii` 建立圓環曲面。選取了兩面作為 `TSplineSurface.BridgeFacesToFaces` 節點的輸入，以及圓環曲面。其餘輸入示範如何進一步調整橋樑:
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`: (選擇性) 刪除邊界橋樑之間的橋樑以防止產生縐摺。
- `keepSubdCreases`: (選擇性) 保留輸入拓樸的子分割縐摺，進而對橋樑起點和終點進行縐摺處理。因為圓環曲面沒有縐摺邊，所以此輸入不會影響形狀。
- `firstAlignVertices`(選擇性) 和 `secondAlignVertices`: 透過指定一對偏移的頂點，橋樑會輕微旋轉。
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## 範例檔案

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
