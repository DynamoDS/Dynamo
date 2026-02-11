## Podrobnosti
Uzel `List.TakeEachNthItem` vytvoří nový seznam obsahující pouze položky ze vstupního seznamu, které jsou v intervalech vstupní hodnoty n. Počáteční bod intervalu je možné změnit zadáním hodnoty vstupu `offset`. Například zadáním hodnoty 3 do n a ponecháním odsazení na výchozí hodnotě 0 se zachovají položky s indexy 2, 5, 8 atd. S odsazením 1 jsou zachovány položky s indexy 0, 3, 6 atd. Všimněte si, že odsazení se „zalamuje“ na druhý konec seznamu. Chcete-li vybrané položky odebrat místo jejich ponechání, najdete potřebné informace u `List.DropEveryNthItem`.

V následujícím příkladu nejprve vygenerujeme seznam čísel pomocí uzlu`Range` a poté zachováme každé druhé číslo pomocí hodnoty 2 jako vstupu pro n.
___
## Vzorový soubor

![List.TakeEveryNthItem](./DSCore.List.TakeEveryNthItem_img.jpg)
