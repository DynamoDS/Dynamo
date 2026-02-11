## 详细
此节点会将对象转化为字符串。第二个输入“格式说明符”控制如何将数值输入转换为其字符串表示形式。
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
