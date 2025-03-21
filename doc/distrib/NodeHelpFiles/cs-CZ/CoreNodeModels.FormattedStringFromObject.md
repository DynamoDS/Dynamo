## Podrobnosti
This node will convert an object to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
Tento vstup `format specifier`by měl být jedním z číselných specifikátorů standardního formátu jazyka C#.

specifikátory formátu by měly mít následující formu:
`<specifier><precision>` například F1

Mezi běžně používané specifikátory formátu patří:
```
G : obecné formátování G 1000.0 -> "1000"
F: označení pevné řádové čárky F4 1000.0 -> "1000.0000"
N : číslo N2 1000 -> "1 000,00"
```

Výchozí hodnota pro tento uzel je `G`, což vytvoří kompaktní, ale proměnnou reprezentaci.

[Podrobnější informace naleznete v dokumentaci společnosti Microsoft.](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## Vzorový soubor

![Formatted String from Object](./CoreNodeModels.FormattedStringFromObject_img.jpg)
