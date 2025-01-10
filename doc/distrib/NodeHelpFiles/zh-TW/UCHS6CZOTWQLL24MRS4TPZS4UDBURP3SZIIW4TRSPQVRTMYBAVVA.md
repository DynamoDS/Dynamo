<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
以下範例在平面基本型的角落頂點使用 `TSplineSurface.UncreaseVertices` 節點。這些頂點在建立曲面時預設會產生縐摺。透過 `TSplineVertex.UVNFrame` 和 `TSplineUVNFrame.Poision` 節點並啟用「展示標籤」選項可識別頂點，然後使用 `TSplineTopology.VertexByIndex` 節點選取角落頂點並取消縐摺。如果形狀是處於平滑模式預覽，則可以預覽此動作的效果。

## 範例檔案

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
