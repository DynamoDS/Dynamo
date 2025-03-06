## Podrobnosti
Uzel `List.GroupByFunction` vrací nový seznam seskupený podle funkce.

Vstup `groupFunction` vyžaduje uzel ve stavu funkce (tj. vrací funkci). To znamená, že nejméně jeden vstup uzlu není připojen. Aplikace Dynamo poté spustí funkci uzlu na každé položce ve vstupním seznamu uzlu `List.GroupByFunction`, aby použila výstup jako mechanismus seskupení.

V následujícím příkladu jsou dva různé seznamy seskupeny pomocí uzlu `List.GetItemAtIndex` jako funkce. Tato funkce vytváří skupiny (nový seznam) z každého indexu nejvyšší úrovně.
___
## Vzorový soubor

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
