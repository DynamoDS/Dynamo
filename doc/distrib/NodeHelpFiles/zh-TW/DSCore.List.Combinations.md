## 深入資訊
`List.Combinations` 會傳回一個巢狀清單，其中包括給定長度的輸入清單中項目的所有可能組合。組合的元素順序無關緊要，因此輸出清單 (0,1) 會被視為是與 (1,0) 相同的組合。如果 `replace` 設定為 True，則項目會放回原始清單中，讓它們能在組合中重複使用。

在以下範例中，我們使用 Code Block 產生一個從 0 到 5，以 1 遞增的數字範圍。我們使用輸入 length 為 3 的 `List.Combinations`，產生組合範圍中 3 個數字的所有不同方式。`replace` 布林值設定為 True，因此將重複使用數字。
___
## 範例檔案

![List.Combinations](./DSCore.List.Combinations_img.jpg)
