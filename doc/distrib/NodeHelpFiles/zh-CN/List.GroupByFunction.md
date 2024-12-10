## 详细
`List.GroupByFunction` 返回按函数分组的新列表。

`groupFunction` 输入需要处于函数状态的节点(即，该节点返回函数)。这意味着节点的至少一个输入未连接。然后，Dynamo 对 `List.GroupByFunction` 的输入列表中每个项运行节点函数，以将输出用作分组机制。

在下面的示例中，使用 `List.GetItemAtIndex` 作为函数以对两个不同的列表进行分组。此函数基于每个顶级索引创建组(新列表)。
___
## 示例文件

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
