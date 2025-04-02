## 详细
“List.GroupBySimilarity”根据元素索引的邻接性和值的相似性来对列表元素进行聚类。要聚类的元素列表可以包含数字(整数和浮点数)或字符串，但不能同时包含两者。

使用“tolerance”输入来确定元素的相似性。对于数字列表，“tolerance”值表示两个数字之间允许的最大差异，以便将它们视为相似。

For string lists, 'tolerance' represents the maximum number of characters that can differ between two strings, using Levenshtein distance for comparison. Maximum tolerance for strings is limited to 10.

“considerAdjacency”布尔输入指示在对图元进行聚类时是否应考虑邻接性。如果为 True，则仅将相似的相邻元素聚集在一起。如果为 False，则无论邻接性如何，都将单独使用相似性来形成聚类。

该节点基于邻接性和相似性输出聚类的值列表，以及原始列表中聚类元素索引的列表。

在下面的示例中，“List.GroupBySimilarity”以两种方式使用: 仅按相似性对字符串列表进行聚类，以及按邻接性和相似性对数字列表进行聚类。
___
## 示例文件

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
