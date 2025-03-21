## Подробности
This node will convert an array to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
Параметр ввода `format specifier` должен быть одним из спецификаторов стандартных числовых форматов C#.

Спецификаторы формата должны иметь следующий формат:
`<спецификатор><точность>` (например, F1)

Далее представлены некоторые наиболее часто используемые спецификаторы формата:
```
G : общее форматирование G 1000.0 -> «1000»
F : обозначение фиксированной точки F4 1000.0 -> «1000.0000»
N : число N2 1000 -> «1 000,00»
```

По умолчанию для этого узла используется спецификатор `G`, который выводит компактное, но переменное представление.

[Подробные сведения см. в документации корпорации Майкрософт.](https://learn.microsoft.com/ru-ru/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## Файл примера

![Formatted String from Array](./CoreNodeModels.FormattedStringFromArray_img.jpg)
