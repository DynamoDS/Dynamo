## 详细
`List.UniqueItems` 通过创建一个仅包含在原始列表中仅出现一次的列表项的新列表，从而删除输入列表中的所有重复项。

在下面的示例中，我们使用 `Math.RandomList` 先生成一个包含介于 0 和 1 之间随机数的列表。然后，我们乘以 10 并使用 `Math.Floor` 操作返回一个包含介于 0 和 9 之间随机整数的列表(其中许多整数重复多次)。我们使用 `List.UniqueItems` 创建一个列表，其中每个整数仅出现一次。输出列表的顺序基于第一个找到的列表项实例。
___
## 示例文件

![List.UniqueItems](./DSCore.List.UniqueItems_img.jpg)
