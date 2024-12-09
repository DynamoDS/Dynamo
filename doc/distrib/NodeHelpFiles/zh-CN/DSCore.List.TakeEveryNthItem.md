## 详细
`List.TakeEveryNthItem` 生成一个新列表，其中仅包含输入列表中按输入 n 值间隔的项。可以使用 `offset` 输入更改间隔的起点。例如，输入 3 到 n 并将 `offset` 保留为默认值 0，将保留索引为 2、5、8 等的项。当 `offset` 为 1 时，将保留索引为 0、3、6 等的项。请注意，`offset`“贯穿”整个列表。要删除选定项而不是保留它们，请参见 `List.DropEveryNthItem`。

在下面的示例中，我们先使用 `Range` 生成一列数字，然后通过将 2 用作 n 的输入来保留每隔一个数字。
___
## 示例文件

![List.TakeEveryNthItem](./DSCore.List.TakeEveryNthItem_img.jpg)
