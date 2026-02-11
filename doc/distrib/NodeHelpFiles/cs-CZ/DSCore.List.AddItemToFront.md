## Podrobnosti
Položka List.AddItemToFront vloží danou položku na začátek daného seznamu. Nová položka má index 0, zatímco indexy původních položek budou posunuty o 1. Berte na vědomí, že pokud je přidávaná položka seznam, přidá se tento seznam jako jeden objekt, čímž vznikne vnořený seznam. Chcete-li sloučit dva seznamy do jednoho seznamu bez vnoření, přejděte na uzel `List.Join`.

V níže uvedeném příkladu vygenerujeme pomocí bloku kódu rozsah čísel od 0 do 5 s krokem 1. Poté přidáme novou položku, číslo 20, na začátek tohoto seznamu pomocí uzlu `List.AddItemToFront.`
___
## Vzorový soubor

![List.AddItemToFront](./DSCore.List.AddItemToFront_img.jpg)
