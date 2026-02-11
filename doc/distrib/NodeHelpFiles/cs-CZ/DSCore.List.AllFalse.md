## Podrobnosti
Uzel `List.AllFalse` vrátí hodnotu False, pokud má libovolná položka v daném seznamu hodnotu True nebo není booleovskou hodnotou. Uzel `List.AllFalse` vrátí hodnotu True pouze v případě, že všechny položky v daném seznamu mají booleovskou hodnotu a tato hodnota je False.

V následujícím příkladu vyhodnotíme seznamy booleovských hodnot pomocí uzlu `List.AllFalse`. První seznam obsahuje hodnotu True, takže je vrácena hodnota False. Druhý seznam obsahuje pouze hodnoty False, takže je vrácena hodnota True. Třetí seznam obsahuje dílčí seznam, který obsahuje hodnotu True, takže je vrácena hodnota False. Poslední uzel vyhodnotí dva dílčí seznamy a vrátí hodnotu False u prvního dílčího seznamu, protože tento obsahuje hodnotu True, a hodnotu True u druhého dílčího seznamu, protože obsahuje pouze hodnoty False.
___
## Vzorový soubor

![List.AllFalse](./DSCore.List.AllFalse_img.jpg)
