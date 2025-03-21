## 深入資訊
This node will convert an object to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
此 `format specifier` 輸入必須是其中一個 C# 標準格式數值指定字。

格式指定字應採用以下形式:
`<specifier><precision>`，例如 F1

一些常用的格式指定字包括:
```
G : 一般格式 G 1000.0 -> "1000"
F : 定點表示法 F4 1000.0 -> "1000.0000"
N : 數字 N2 1000 -> "1,000.00"
```

此節點的預設值為 `G`，這會輸出一個精簡但可變的表現法。

[請參閱 Microsoft 文件以取得更多詳細資訊。](https://learn.microsoft.com/zh-tw/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## 範例檔案

![Formatted String from Object](./CoreNodeModels.FormattedStringFromObject_img.jpg)
