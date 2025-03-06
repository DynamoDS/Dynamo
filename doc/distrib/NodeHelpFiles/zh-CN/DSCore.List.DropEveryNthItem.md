## 详细
`List.DropEveryNthItem` 按输入 n 值的间隔从输入列表中删除列表项。间隔的起点可以使用 `offset` 输入进行更改。例如，在 n 中输入 3 并将 `offset` 保留为默认值 0 时，将删除索引为 2、5、8 等的项。当 `offset` 为 1 时，将删除索引为 0、3、6 等的项。请注意，`offset`“贯穿”整个列表。要保留选定项而不是删除它们，请参见 `List.TakeEveryNthItem`。

在下面的示例中，我们先使用 `Range` 生成一个数字列表，然后使用 2 作为 `n` 输入删除每隔一个数字。
___
## 示例文件

![List.DropEveryNthItem](./DSCore.List.DropEveryNthItem_img.jpg)
