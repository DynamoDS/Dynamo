## Podrobnosti
Uzel If se chová jako podmínkový řídicí uzel. Vstup 'test' přijímá booleovskou hodnotu, zatímco vstupy 'true'a 'false' přijímají libovolný datový typ. Pokud je testovaná hodnota 'true', uzel vrátí položku ze vstupu 'true', a pokud je testovaná hodnota 'false', uzel vrátí položku ze vstupu 'false'. V následujícím příkladu nejprve vygenerujeme seznam náhodných čísel v rozmezí od nuly do 99. Počet položek v seznamu je řízen celočíselným posuvníkem. Pomocí bloku kódu se vzorcem 'x%a==0' otestujeme dělitelnost druhým číslem, které je určeno druhým posuvníkem. Tím se vygeneruje seznam booleovských hodnot, které určují, zda jsou položky v náhodném seznamu dělitelné číslem určeným druhým celočíselným posuvníkem. Tento seznam booleovských hodnot se používá jako vstup 'test' pro uzel If. Jako vstup 'true' použijeme výchozí kouli a jako vstup 'false' použijeme výchozí kvádr. Výsledkem uzlu If je buď seznam koulí, nebo seznam kvádrů. Nakonec pomocí uzlu Translate roztáhneme položky seznamu geometrií více od sebe.

Uzel IF se replikuje u všech uzlů JAKO KDYBY BYL NASTAVEN NA HODNOTU SHORTEST. Důvod tohoto chování je možné vidět v přiložených příkladech, zvlášť vzhledem k výsledkům, které vzniknou, pokud použijeme hodnotu LONGEST na uzel formula a je předána „krátká“ větev dané podmínky. Tyto změny byly provedeny také kvůli předvídatelnosti chování, pokud se použijí samostatné booleovské vstupy nebo seznam booleovských hodnot.
___
## Vzorový soubor

![If](./CoreNodeModels.Logic.RefactoredIf_img.jpg)

