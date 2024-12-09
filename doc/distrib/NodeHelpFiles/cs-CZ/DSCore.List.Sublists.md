## Podrobnosti
Uzel `List.Sublists` vrací řadu dílčích seznamů z daného seznamu podle vstupního rozsahu a odsazení. Rozsah určuje prvky vstupního seznamu, které jsou umístěny do prvního dílčího seznamu. Odsazení se použije na rozsah a nový rozsah určuje druhý dílčí seznam. Tento proces se opakuje a zvyšuje počáteční index rozsahu o dané odsazení, dokud není výsledný dílčí seznam prázdný.

V následujícím příkladu začneme s rozsahem čísel od 0 do 9. Rozsah 0 až 5 se použije jako rozsah dílčího seznamu s odsazením 2. Ve výstupu vnořených dílčích seznamů první seznam obsahuje prvky s indexy v rozsahu 0..5 a druhý seznam obsahuje prvky s indexy 2..7. Jak se toto bude opakovat, následné dílčí seznamy budou čím dál kratší, protože konec rozsahu překročí délku počátečního seznamu.
___
## Vzorový soubor

![List.Sublists](./DSCore.List.Sublists_img.jpg)
