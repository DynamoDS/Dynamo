## 深入資訊
FromPixels (有寬度和高度) 會從輸入的顏色展開清單建立影像，其中每種顏色都會變成一個像素。寬度乘以高度應等於顏色總數。在以下範例中，我們先使用 ByARGB 節點建立顏色清單。Code Block 會建立 0 到 255 的值範圍，當連接至 r 和 g 輸入時會產生一系列從黑色到黃色的顏色。我們建立寬度為 8 的影像。使用 Count 節點和 Divide 節點決定影像的高度。使用 Watch Image 節點可以預覽建立的影像。
___
## 範例檔案

![FromPixels (colors, width, height)](./DSCore.IO.Image.FromPixels(colors,%20width,%20height)_img.jpg)

