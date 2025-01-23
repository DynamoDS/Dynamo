## 深入資訊
GetColorAtParameter 接受一個輸入 2D 顏色範圍，並以範圍 0 到 1 傳回指定的 UV 參數的顏色清單。在以下範例中，我們先對顏色清單和參數清單使用 ByColorsAndParameters 節點建立一個 2D 顏色範圍來設定範圍。使用 Code Block 產生一個 0 到 1 之間的數字範圍，當作 UV.ByCoordinates 節點中的 u 和 v 輸入。此節點的交織設定為「笛卡兒積」。以類似方式建立一組立方體，亦即使用「笛卡兒積」交織的 Point.ByCoordinates 節點建立一個立方體陣列。然後，我們對立方體陣列和從 GetColorAtParameter 節點得到的顏色清單使用 Display.ByGeometryColor 節點。
___
## 範例檔案

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

