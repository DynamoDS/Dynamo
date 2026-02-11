## Podrobnosti
Uzel `List.Chop` rozdělí daný seznam na menší seznamy podle seznamu vstupních délek vyjádřených celými čísly. První vnořený seznam obsahuje počet prvků určený prvním číslem ve vstupu `lengths`. Druhý vnořený seznam obsahuje počet prvků určený druhým číslem ve vstupu `lengths` atd. Uzel `List.Chop` opakuje poslední číslo ve vstupu `lengths`, dokud nejsou všechny prvky ze vstupního seznamu rozděleny.

V následujícím příkladu vygenerujeme pomocí bloku kódu rozsah čísel mezi 0 a 5 s krokem 1. Tento seznam obsahuje 6 prvků. Pomocí druhého bloku kódu vytvoříme seznam délek, podle kterých bude první seznam rozdělen. První číslo v tomto seznamu je 1, které uzel `List.Chop` použije k vytvoření vnořeného seznamu s 1 položkou. Druhé číslo je 3 a vytvoří vnořený seznam se 3 položkami. Vzhledem k tomu, že nejsou určeny žádné další délky, uzel `List.Chop` zahrne všechny zbývající položky do třetího a posledního vnořeného seznamu.
___
## Vzorový soubor

![List.Chop](./DSCore.List.Chop_img.jpg)
