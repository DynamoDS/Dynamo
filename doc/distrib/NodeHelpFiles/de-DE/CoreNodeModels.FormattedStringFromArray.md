## Im Detail
Dieser Block konvertiert ein Objekt in eine Zeichenfolge. Die zweite Eingabe `format specifier` steuert, wie numerische Eingaben in ihre Zeichenfolgendarstellungen konvertiert werden.
Diese Eingaben für `format specifier` sollten numerische C#-Standardformatspezifizierer sein.

Formatspezifizierer sollten die folgende Form haben:
`<specifier><precision>,` zum Beispiel F1

Einige häufig verwendete Formatspezifizierer sind:
```
G: allgemeine Formatierung G 1000.0 -> "1000"
F: Festkommanotation F4 1000.0 -> "1000.0000"
N: Anzahl N2 1000 -> "1,000.00"
```

Die Vorgabe für diesen Block ist `G`, wodurch eine kompakte, aber variable Darstellung ausgegeben wird.

[Weitere Informationen finden Sie in der Microsoft-Dokumentation.](https://learn.microsoft.com/de-de/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
