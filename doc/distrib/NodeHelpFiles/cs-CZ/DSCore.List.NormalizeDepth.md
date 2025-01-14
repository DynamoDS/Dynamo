## Podrobnosti
Uzel `List.NormalizeDepth` vrací nový seznam jednotné hloubky o zadané úrovni nebo hloubce seznamu.

Podobně jako u uzlu `List.Flatten` můžete použít uzel `List.NormalizeDepth` k vrácení jednorozměrného seznamu (seznamu s jednou úrovní). Je však možné jej použít také k přidání úrovní do seznamu. Uzel normalizuje vstupní seznam na hloubku dle vašeho výběru.

V následujícím příkladu je možné normalizovat seznam obsahující 2 seznamy nestejné hloubky na různé úrovně pomocí celočíselného posuvníku. Normalizací hloubek na různých úrovních seznam zvětší nebo zmenší hloubku, je však vždy jednotný. Seznam úrovně 1 vrací jeden seznam prvků, zatímco seznam úrovně 3 vrací 2 úrovně dílčích seznamů.
___
## Vzorový soubor

![List.NormalizeDepth](./DSCore.List.NormalizeDepth_img.jpg)
