## Podrobnosti
Uzel IsNull vrátí booleovskou hodnotu podle toho, zda je objekt roven hodnotě null. V níže uvedeném příkladu je nakreslena osnova kružnic s proměnnými poloměry podle úrovně červené v bitmapě. Tam, kde není žádná hodnota červené, není nakreslena žádná kružnice a v seznamu kružnic se vrací hodnota null. Pokud tento list předáme do uzlu IsNull, vrátí se seznam booleovských hodnot, v němž hodnota true představuje všechna umístění s hodnotou null. Tento seznam booleovských hodnot je možné použít v uzlu List.FilterByBoolMask k vrácení seznamu bez hodnot null.
___
## Vzorový soubor

![IsNull](./DSCore.Object.IsNull_img.jpg)

