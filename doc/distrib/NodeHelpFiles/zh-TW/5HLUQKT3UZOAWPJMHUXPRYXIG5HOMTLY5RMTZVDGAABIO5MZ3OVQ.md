<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## 深入資訊
`Surface.Thicken (surface, thickness, both_sides)` 會根據 `thickness` 輸入偏移曲面，並將兩端覆蓋，建立封閉的實體。此節點還有另外一個輸入指定是否要在兩側增厚。`both_sides` 輸入接受布林值: True 是在兩側增厚，False 是在一側增厚。請注意，`thickness` 參數決定最終實體的總厚度，因此如果 `both_sides` 設定為 True，結果將從原始曲面兩側各偏移一半的輸入厚度。

在以下範例中，我們先使用 `Surface.BySweep2Rails` 建立曲面，然後使用數字滑棒決定 `Surface.Thicken` 節點的 `thickness` 輸入以建立實體。布林切換控制要在兩側增厚還是只增厚一側。

___
## 範例檔案

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
