## Informacje szczegółowe
Ten węzeł konwertuje obiekt na ciąg. Druga pozycja danych wejściowych `format specifier` steruje sposobem konwertowania liczbowych danych wejściowych na ich reprezentacje w postaci ciągów.
Ta pozycja danych wejściowych `format specifier` powinna być jednym ze standardowych specyfikatorów liczbowych formatu języka C#.

Specyfikatory formatu powinny mieć postać:
`<specyfikator><dokładność>`, na przykład F1

Niektóre często używane specyfikatory formatu to:
```
G: formatowanie ogólne G 1000,0 -> "1000"
F: zapis stałoprzecinkowy F4 1000,0 -> "1000,0000"
N: liczba N2 1000 -> "1000,00"
```

Wartość domyślna tego węzła to `G`, która zwróci kompaktową, ale zmienną reprezentację.

[Szczegółowe informacje można znaleźć w dokumentacji firmy Microsoft.](https://learn.microsoft.com/pl-pl/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
