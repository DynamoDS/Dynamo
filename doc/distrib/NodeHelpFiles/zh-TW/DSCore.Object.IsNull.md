## 深入資訊
IsNull 會根據物件是否為空值而傳回布林值。在以下範例中，根據點陣圖中的紅色程度以不同半徑繪製圓格線。沒有紅色值的位置，就不繪製圓，並在圓清單中傳回空值。將此清單傳入 IsNull 會傳回布林值清單，true 表示每個空值的位置。此布林清單可用於 List.FilterByBoolMask 傳回不含空值的清單。
___
## 範例檔案

![IsNull](./DSCore.Object.IsNull_img.jpg)

