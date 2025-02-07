## 深入資訊
ByColorsAndParameters 會從輸入顏色清單和以範圍 0 到 1 指定的 UV 參數的對應清單建立一個 2D 顏色範圍。在以下範例中，我們使用 Code Block 建立三種不同的顏色 (在此範例中是純綠色、純紅色和純藍色)，並將它們合併成一個清單。我們使用另一個 Code Block 建立三個 UV 參數，每個顏色一個參數。使用這兩個清單作為 ByColorsAndParameters 節點的輸入。我們後面使用一個 GetColorAtParameter 節點，加上 Display.ByGeometryColor 節點，在一組立方體當中顯示 2D 顏色範圍。
___
## 範例檔案

![ByColorsAndParameters](./DSCore.ColorRange.ByColorsAndParameters_img.jpg)

