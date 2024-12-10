## Podrobnosti
Uzel `List.Combinations` vrací vnořený seznam, který obsahuje všechny možné kombinace položek ve vstupním seznamu s danou délkou. U kombinací nezáleží na pořadí prvků, takže výstupní seznam (0,1) je považován za stejnou kombinaci jako (1,0). Pokud je nastaven vstup `replace` na hodnotu True, položky budou nahrazeny v rámci původního seznamu, což umožní jejich opakované použití v kombinaci.

V následujícím příkladu pomocí bloku kódu vygenerujeme rozsah čísel od 0 do 5 s krokem 1. Pomocí uzlu `List.Combine` se vstupní délkou 3 vygenerujeme všechny různé způsoby kombinací 3 čísel v rámci rozsahu. Booleovská hodnota `replace` je nastavena na hodnotu True, takže čísla budou použita opakovaně.
___
## Vzorový soubor

![List.Combinations](./DSCore.List.Combinations_img.jpg)
