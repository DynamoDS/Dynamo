<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
`TSplineSurface.AddReflections` 透過對輸入 `tSplineSurface` 套用一個或多個反射來建立新的 T 雲形線曲面。布林輸入 `weldSymmetricPortions` 決定要將反射產生的縐摺邊變平滑還是保留。

以下範例說明如何使用 `TSplineSurface.AddReflections` 節點在 T 雲形線曲面增加多個反射。建立兩個反射 - 軸向和徑向。基準幾何圖形是使用弧路徑的掃掠形狀中的 T 雲形線曲面。在清單中接合兩個反射，並用作 `TSplineSurface.AddReflections` 節點的輸入，以及要反射的基準幾何圖形。TSplineSurfaces 會結合在一起，產生無縐摺邊的平滑 TSplineSurface。使用 `TSplineSurface.MoveVertex` 節點移動一個頂點進一步改變曲面。由於反射是作用到 T 雲形線曲面，因此會產生 16 次的頂點移動。

## 範例檔案

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
