## Podrobnosti
Uzel `List.Transpose` zamění řádky a sloupce v seznamu seznamů. Například seznam obsahující 5 dílčích seznamů po 10 položkách, bude převeden na 10 seznamů po 5 položkách. Podle potřeby se vkládají hodnoty null, aby se zajistilo, že každý dílčí seznam bude mít stejný počet položek.

V tomto příkladu vygenerujeme seznam čísel od 0 do 5 a další seznam písmen od A do E. Poté je zkombinujeme pomocí uzlu `List.Create`. Pomocí uzlu `List.Transpose` se vygeneruje 6 seznamů po 2 položkách, jedno číslo a jedno písmeno na jeden seznam. Všimněte si, že vzhledem k tomu, že jeden z původních seznamů byl delší než druhý, uzel `List.Transpose` vložil hodnotu null pro nespárovanou položku.
___
## Vzorový soubor

![List.Transpose](./DSCore.List.Transpose_img.jpg)
