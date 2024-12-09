## 深入資訊
如果給定清單中有任何一個項目為 False 或不是布林值，`List.AllTrue` 會傳回 False。只有當給定清單中的所有項目都為布林值且為 True 時，`List.AllTrue` 才會傳回 True。

在以下範例中，我們使用 `List.AllTrue` 演算布林值清單。第一個清單有 False 值，因此傳回 False。第二個清單只有 True 值，因此傳回 True。第三個清單有包含 False 值的子清單，因此傳回 False。最後一個節點會演算兩個子清單，第一個子清單因為有 False 值，所以傳回 False，第二個子清單因為只有 True 值，所以傳回 True。
___
## 範例檔案

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
