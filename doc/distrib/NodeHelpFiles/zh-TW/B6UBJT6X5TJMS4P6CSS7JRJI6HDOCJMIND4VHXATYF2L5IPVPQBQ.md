<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
`TSplineSurface.RemoveReflections` 會從輸入 `tSplineSurface` 移除反射。移除反射不會修改形狀，但會中斷幾何圖形反射部分之間的相依性，讓您可以獨立編輯各部分。

以下範例先套用軸向和徑向反射建立 T 雲形線曲面，然後將曲面傳入 `TSplineSurface.RemoveReflections` 節點來移除反射。為了說明這會如何影響之後的變更，使用 `TSplineSurface.MoveVertex` 節點移動其中一個頂點。由於反射已從曲面移除，因此只修改了一個頂點。

## 範例檔案

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
