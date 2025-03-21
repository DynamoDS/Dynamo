## In profondità
This node will convert an object to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
Gli input `format specifier` devono essere uno degli indicatori numerici di formato standard C#.

Gli indicatori di formato devono essere nel formato:
'<specifier><precision>' ad esempio F1

Alcuni indicatori di formato comunemente utilizzati sono:
```
G: formattazione generale G 1000.0 -> "1000"
F: notazione a virgola fissa F4 1000.0 -> "1000.0000"
N: numero N2 1000 -> "1.000,00"
```

Il valore di default per questo nodo è `G`, che produrrà una rappresentazione compatta ma variabile.

[Per informazioni più dettagliate, vedere la documentazione di Microsoft.](https://learn.microsoft.com/it-it/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## File di esempio

![Formatted String from Object](./CoreNodeModels.FormattedStringFromObject_img.jpg)
