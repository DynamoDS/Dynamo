## 深入資訊
`List.AddItemToFront` 會將給定項目插入給定清單的開頭。新項目的索引為 0，而原始項目均以索引 1 移動。請注意，如果要加入的項目是清單，則會當作單一物件加入而產生巢狀清單。若要將兩個清單合併成單一展開清單，請參閱 `List.Join`。

在以下範例中，我們使用 Code Block 產生一個從 0 到 5，以 1 遞增的數字範圍，然後使用 `List.AddItemToFront` 將新項目 (數字 20) 加到該清單的開頭。
___
## 範例檔案

![List.AddItemToFront](./DSCore.List.AddItemToFront_img.jpg)
