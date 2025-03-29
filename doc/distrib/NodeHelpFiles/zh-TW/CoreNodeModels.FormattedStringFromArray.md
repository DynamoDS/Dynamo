## 深入資訊
此節點會將陣列轉換為字串。第二個輸入 `format specifier` 控制數值輸入如何轉換為其字串表現法。
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

![Formatted String from Array](./CoreNodeModels.FormattedStringFromArray_img.jpg)
