## Podrobnosti
Uzel `List.GroupBySimilarity` seskupuje prvky seznamu podle přilehlosti jejich indexů a podobnosti jejich hodnot. Seznam prvků, které mají být seskupeny, může obsahovat buď čísla (celá čísla a čísla s plovoucí desetinnou čárkou), nebo řetězce, ale ne kombinaci obou.

Pomocí vstupu `tolerance` určete podobnost prvků. U seznamů čísel představuje hodnota `tolerance` maximální přípustný rozdíl mezi dvěma čísly, aby byla považována za podobná.

U seznamů řetězců představuje vstup 'tolerance' maximální počet znaků, které se mohou mezi dvěma řetězci lišit, přičemž ke srovnání se použije Levenštejnova vzdálenost. Maximální tolerance řetězců je nastavena na hodnotu 10.

Booleovský vstup `considerAdjacency` označuje, zda má být při seskupování prvků zohledněna přilehlost. Pokud má hodnotu True, budou seskupeny dohromady pouze sousední prvky, které jsou podobné. Pokud má hodnotu False, k vytvoření shluků se použije samotná podobnost bez ohledu na přilehlost.

Uzel vytvoří na výstupu seznam seznamů seskupovaných hodnot podle přilehlosti a podobnosti a také seznam seznamů indexů seskupovaných prvků v původním seznamu.

V následující ukázce se uzel`List.GroupBySimilarity` používá dvěma způsoby: k seskupení seznamu řetězců pouze podle podobnosti a ke shlukování seznamu čísel podle přilehlosti a podobnosti.
___
## Vzorový soubor

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
