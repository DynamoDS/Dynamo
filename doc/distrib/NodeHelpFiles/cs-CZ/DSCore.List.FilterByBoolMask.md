## Podrobnosti
Uzel `List.FilterByBoolMask` přijímá dva seznamy jako vstupy. První seznam je rozdělen na dva samostatné seznamy podle příslušného seznamu booleovských (True nebo False) hodnot. Položky ze vstupu `list`, které odpovídají hodnotě True ve vstupu `mask`, jsou směrovány k výstupu s názvem In, zatímco položky, které odpovídají hodnotě False, jsou směrovány k výstupu s názvem `out`.

V následujícím příkladu se pomocí uzlu `List.FilterByBoolMask` vybere v seznamu materiálů dřevo a laminát. Nejprve porovnáme dva seznamy k nalezení odpovídajících položek a poté pomocí operátoru `Or` zkontrolujeme, zda jsou v seznamu položky s hodnotou True. Poté položky seznamy filtrujeme podle toho, zda se jedná o dřevo, laminát nebo něco jiného.
___
## Vzorový soubor

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
