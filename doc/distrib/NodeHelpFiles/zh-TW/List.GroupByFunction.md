## 深入資訊
`List.GroupByFunction` 會傳回依函數分組的新清單。

`groupFunction` 輸入需要一個處於函數狀態 (即傳回函數) 的節點。這表示至少要有一個節點的輸入未連接。然後，Dynamo 會對 `List.GroupByFunction` 的輸入清單中的每個項目執行節點函數，將輸出當作分組機制。

在以下範例中，使用 `List.GetItemAtIndex` 作為函數將兩個不同的清單分組。此函數會從每個頂層索引建立群組 (新清單)。
___
## 範例檔案

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
