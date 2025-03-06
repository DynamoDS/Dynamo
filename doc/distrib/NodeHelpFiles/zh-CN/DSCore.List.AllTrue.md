## 详细
如果给定列表中的任一项是 False 或不是布尔值，`List.AllTrue` 返回 False。仅当给定列表中的每一项是布尔值且为 True 时，`List.AllTrue` 返回 True。

在下面的示例中，我们使用 `List.AllTrue` 来计算布尔值列表。第一个列表有 False 值，因此返回 False。第二个列表只有 True 值，因此返回 True。第三个列表有子列表包含 False 值，因此返回 False。最后一个节点计算两个子列表，对第一个子列表返回 False (因为它有 False 值)，对第二个子列表返回 True (因为它只有 True 值)。
___
## 示例文件

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
