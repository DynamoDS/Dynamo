## Podrobnosti
Uzel `List.UniqueItems` odebere všechny duplicitní položky ze vstupního seznamu vytvořením nového seznamu, který obsahuje pouze položky, které se v původním seznamu vyskytují pouze jednou.

V následujícím příkladu nejprve vygenerujeme pomocí uzlu `Math.RandomList` seznam náhodných čísel v rozmezí 0 až 1. Poté tato čísla vynásobíme 10 a pomocí uzlu `Math.Floor` vrátíme seznam náhodných celých čísel v rozmezí 0 až 9, přičemž mnohá čísla se vícekrát opakují. Pomocí uzlu `List.UniqueItems` vytvoříme seznam, ve kterém se každé celé číslo vyskytuje pouze jednou. Pořadí výstupního seznamu je založeno na první nalezené instanci položky.
___
## Vzorový soubor

![List.UniqueItems](./DSCore.List.UniqueItems_img.jpg)
