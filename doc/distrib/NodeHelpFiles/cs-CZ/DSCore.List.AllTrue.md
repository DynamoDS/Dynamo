## Podrobnosti
Uzel `List.AllTrue` vrátí hodnotu False, pokud libovolná položka v daném seznamu má hodnotu False nebo nemá booleovskou hodnotu. Uzel `List.AllTrue` vrací hodnotu True pouze v případě, že všechny položky v daném seznamu mají booleovskou hodnotu a tato hodnota je True.

V následujícím příkladu vyhodnotíme seznamy booleovských hodnot pomocí uzlu `List.AllTrue`. První seznam obsahuje hodnotu False, takže je vrácena hodnota False. Druhý seznam obsahuje pouze hodnoty True, takže je vrácena hodnota True. Třetí seznam obsahuje dílčí seznam, který obsahuje hodnotu False, takže je vrácena hodnota False. Poslední uzel vyhodnotí dva dílčí seznamy a vrátí hodnotu False u prvního dílčího seznamu, protože tento obsahuje hodnotu False, a hodnotu True u druhého dílčího seznamu, protože obsahuje pouze hodnoty True.
___
## Vzorový soubor

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
