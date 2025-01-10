## 深入資訊
`List.ShiftIndices` 會依 `amount` 輸入移動清單中項目的位置。`amount` 輸入中的正數會將數字向上移動，負數會將索引向後移動。項目會折繞回來，因此清單後面的項目會折繞至開頭。

在以下範例中，我們先使用 `Range` 產生清單，然後將索引向前移動 3。原始清單中的最後 3 個數字會折繞回來，成為新清單中的前 3 個數字。
___
## 範例檔案

![List.ShiftIndices](./DSCore.List.ShiftIndices_img.jpg)
