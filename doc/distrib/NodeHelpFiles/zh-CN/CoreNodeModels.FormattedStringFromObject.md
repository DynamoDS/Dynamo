## 详细
This node will convert an object to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
此“格式说明符”输入应该是 C# 标准格式数字说明符之一。

格式说明符应采用以下形式:
`<specifier><precision>` 例如 F1

一些常用的格式说明符包括:
```
G : 常规格式 G 1000.0 -> "1000"
F : 定点法 F4 1000.0 -> "1000.0000"
N : 编号 N2 1000 -> "1,000.00"
```

此节点的默认值为“G”，这将输出紧凑但可变的表示。

[有关详细信息，请参见 Microsoft 文档。](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## 示例文件

![Formatted String from Object](./CoreNodeModels.FormattedStringFromObject_img.jpg)
