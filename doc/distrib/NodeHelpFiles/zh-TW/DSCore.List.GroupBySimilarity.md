## 深入資訊
`List.GroupBySimilarity` 會根據清單索引的相鄰性和其值的相似性群集清單元素。要群集的元素清單可以包含數字 (整數和浮點數) 或字串，但不能同時包含兩者。

使用 `tolerance` 輸入判斷元素的相似性。如果是數字清單，`tolerance` 值表示兩個數字之間被視為相似的最大允許差異。

如果是字串清單，`tolerance` 表示兩個字串之間可以不同的最大字元數 (使用 Levenshtein 距離進行比較)。字串的最大公差限制為 10。

`considerAdjacency` 布林輸入表示在群集元素時是否應考慮相鄰性。如果為 True，則只有相似的相鄰元素才會群集在一起。如果為 False，則無論相鄰性如何，都會單獨使用相似性來形成群集。

節點會根據相鄰性和相似性輸出群集值清單的清單，以及原始清單中群集元素索引清單的清單。

以下範例以兩種方式使用 `List.GroupBySimilarity`: 僅依相似性群集字串清單，以及依相鄰性和相似性群集數字清單。
___
## 範例檔案

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
