## In profondità
Questo nodo convertirà un oggetto in una stringa. Il secondo input `format specifier` controlla il modo in cui gli input numerici vengono convertiti nelle rispettive rappresentazioni di stringa.
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
