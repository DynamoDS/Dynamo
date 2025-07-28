## 深入資訊
`List.TakeEveryNthItem` 會產生一個新清單，只包含輸入清單中位於輸入 n 值間隔的項目。間隔的起點可以使用 `offset` 輸入變更。例如，n 輸入 3 並讓 offset 保留為預設 0 將保留索引 2、5、8 等的項目。offset 為 1 時，會保留索引 0、3、6 等的項目。請注意，offset 會「折繞」整個清單。若要移除選取的項目而非保留，請參閱 `List.DropEveryNthItem`。

在以下範例中，我們先使用 `Range` 產生數字清單，然後使用 2 作為 n 的輸入保留其他每個數字。
___
## 範例檔案

![List.TakeEveryNthItem](./DSCore.List.TakeEveryNthItem_img.jpg)
