## 深入資訊
ReplaceByCondition 接受給定清單，並以給定條件演算每個項目。如果條件演算為「true」，則輸出清單中對應的項目將取代為 replaceWith 輸入指定的項目。在以下範例中，我們使用 Formula 節點並輸入公式 x%2==0，此公式會找出給定項目除以 2 後的餘數，然後檢查該餘數是否等於零。此公式在偶數整數時會傳回「true」。請注意，輸入 x 留空。使用此公式作為 ReplaceByCondition 節點中的條件，會產生一個輸出清單，其中每個偶數都會被指定的項目取代，在此範例中為整數 10。
___
## 範例檔案

![ReplaceByCondition](./CoreNodeModels.HigherOrder.Replace_img.jpg)

