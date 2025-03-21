## 詳細
This node will convert an object to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
この `format specifier` 入力は、C# 標準書式の数値指定子の 1 つである必要があります。

書式指定子は次の形式にする必要があります。
`<specifier><precision>` (例: F1)

一般的に使用される書式指定子には次のようなものがあります。
```
G : 一般書式 G 1000.0 -> "1000"
F : 固定小数点表記 F4 1000.0 -> "1000.0000"
N : 数値 N2 1000 -> "1,000.00"
```

このノードの既定は `G` で、コンパクトな可変表現を出力します。

[詳細については、Microsoft のドキュメントを参照してください。] (https://learn.microsoft.com/ja-jp/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## サンプル ファイル

![Formatted String from Object](./CoreNodeModels.FormattedStringFromObject_img.jpg)
