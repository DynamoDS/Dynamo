## 深入資訊
`List.DropEveryNthItem` 會以輸入 n 值的間隔從輸入清單中移除項目。間隔的起點可以使用 `offset` 輸入變更。例如，n 輸入 3 並讓 offset 保留為預設 0 將移除索引 2、5、8 等的項目。offset 為 1 時，會移除索引 0、3、6 等的項目。請注意，offset 會「折繞」整個清單。若要保留選取的項目而非移除，請參閱 `List.TakeEveryNthItem`。

在以下範例中，我們先使用 `Range` 產生一個數字清單，然後使用 2 作為 `n` 的輸入來移除其他每個數字。
___
## 範例檔案

![List.DropEveryNthItem](./DSCore.List.DropEveryNthItem_img.jpg)
