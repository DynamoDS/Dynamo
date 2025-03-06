<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## 深入資訊
`TSplineSurface.BridgeToFacesToEdges` 會將相同曲面或兩個不同曲面的一組邊與一組面連接。組成面的邊數必須相符，或為橋樑另一端邊數的倍數。節點需要的輸入如下所述: 只需要前三個輸入就能產生橋樑，其餘輸入為選擇性。產生的曲面是第一組邊所屬曲面的子系。

- `TSplineSurface`: 要橋接的曲面
- `firstGroup`: 從 TSplineSurface 中選取的面
- `secondGroup`: 從相同 T 雲形線曲面或不同 T 雲形線曲面選取的邊。邊數必須相符，或為橋樑另一端邊數的倍數。
- `followCurves`: (選擇性) 沿其橋接的曲線。如果沒有此輸入，將沿直線橋接
- `frameRotations`: (選擇性) 連接所選邊的橋樑擠出旋轉數。
- `spansCounts`: (選擇性) 連接所選邊的橋樑擠出跨距數/段數。如果跨距數太低，除非增加值，否則某些選項可能無法使用。
- `cleanBorderBridges`: (選擇性) 刪除邊界橋樑之間的橋樑以防止產生縐摺
- `keepSubdCreases`: (選擇性) 保留輸入拓樸的子分割縐摺，進而對橋樑起點和終點進行縐摺處理
- `firstAlignVertices` (選擇性) 和 `secondAlignVertices`: 強制兩組頂點之間對齊，而不是自動選擇連接最接近的頂點對。
- `flipAlignFlags`: (選擇性) 反轉要對齊的頂點方向


以下範例使用 `TSplineTopology.VertexByIndex` 和 `TSplineTopology.FaceByIndex` 節點建立兩個 T 雲形線平面，並且收集一些邊和面。為了建立橋樑，使用這些面和邊作為 `TSplineSurface.BrideFacesToEdges` 節點的輸入，搭配其中一個曲面，就會建立橋樑。透過編輯 `spansCounts` 輸入，在橋樑中加入更多跨距。如果使用曲線作為 `followCurves` 的輸入，橋樑就會跟著提供曲線的方向。`keepSubdCreases`、`frameRotations`、`firstAlignVertices` 和 `secondAlignVertices` 輸入示範如何微調橋樑形狀。

## 範例檔案

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
