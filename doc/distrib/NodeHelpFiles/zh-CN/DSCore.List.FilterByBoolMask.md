## 详细
`List.FilterByBoolMask ` 将两个列表用作输入。第一个列表根据相应的布尔值(True 或 False)列表拆分为两个单独的列表。`list` 输入中对应于 `mask` 输入中 True 的列表项将定向到标有 `In` 的输出，而对应于 False 值的列表项将定向到标有 `out` 的输出。

在下面的示例中，`List.FilterByBoolMask` 用于从材质列表中挑出木材和层压板。我们先比较两个列表以查找匹配项，然后使用 `Or` 运算符寻找 True 列表项。然后，根据列表项是木材还是层压板或其他内容来过滤这些列表项。
___
## 示例文件

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
