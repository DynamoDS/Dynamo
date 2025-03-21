## Em profundidade
This node will convert an object to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
Essas entradas do `format specifier` devem ser um dos especificadores numéricos de formato padrão c#.

Os especificadores de formato devem estar no formato:
`<specifier><precision>`, por exemplo, F1

Alguns especificadores de formato comumente usados são:
```
G : formatação geral G 1000.0 -> “1000”
F: notação de ponto fixo F4 1000.0 -> “1000.0000”
N : número N2 1000 -> “1,000.00”
```

O padrão para esse nó é `G`, que produzirá uma representação compacta, mas variável.

[Consulte a documentação da Microsoft para obter informações mais detalhadas.](https://learn.microsoft.com/pt-br/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## Arquivo de exemplo

![Formatted String from Object](./CoreNodeModels.FormattedStringFromObject_img.jpg)
