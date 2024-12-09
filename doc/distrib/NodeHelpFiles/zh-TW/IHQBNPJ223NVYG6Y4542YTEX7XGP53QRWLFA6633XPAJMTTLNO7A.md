<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## 深入資訊
`Surface.TrimWithEdgeLoops` 使用一條或多條封閉的 PolyCurves (必須全部位於指定公差範圍內的曲面上) 修剪曲面。如果需要從輸入曲面剪掉一個或多個孔，則必須為曲面邊界指定一個外迴路，為每個孔指定一個內迴路。如果需要剪掉曲面邊界與孔之間的區域，舊只要提供每個孔的迴路。如果是沒有外迴路 (例如球形曲面) 的週期曲面，可以透過反轉迴路曲線的方向控制修剪的區域。

tolerance 是決定曲線兩端是否重合以及曲線和曲面是否重合時使用的公差。提供的公差不能小於建立輸入 PolyCurves 時使用的任何公差。預設值 0.0 表示將使用建立輸入 PolyCurves 時使用的最大公差。

以下範例從曲面剪掉兩個迴路，傳回以藍色亮顯的兩個新曲面。使用數字滑棒可調整新曲面的造型。

___
## 範例檔案

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
