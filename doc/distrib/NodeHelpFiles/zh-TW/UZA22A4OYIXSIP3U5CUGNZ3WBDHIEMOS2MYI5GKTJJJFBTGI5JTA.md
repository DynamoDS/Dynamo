<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## 深入資訊
以下範例使用 `TSplineSurface.ByCombinedTSplineSurfaces` 節點將 T 雲形線曲面的兩半接合成一個。沿著鏡射平面的頂點會重疊，使用 `TSplineSurface.MoveVertices` 節點移動其中一個頂點時，頂點就會變為可見。為了修復這個狀況，使用 `TSplineSurface.WeldCoincidentVertices` 節點執行接合。移動頂點的結果現在變得不同，已平移到旁邊方便檢視。
___
## 範例檔案

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
