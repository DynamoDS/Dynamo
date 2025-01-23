<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## Podrobnosti
Uzel `Surface.TrimWithEdgeLoops` ořízne povrch pomocí kolekce jednoho nebo více uzavřených objektů PolyCurve, které musí všechny ležet na povrchu v rámci určené tolerance. Pokud je třeba ze vstupního povrchu oříznout jednu nebo více děr, musí být pro hranici povrchu určena jedna vnější smyčka a jedna vnitřní smyčka pro každou díru. Pokud je třeba oříznout oblast mezi hranicí povrchu a děrami, musí být zadána pouze smyčka pro každou díru. U periodického povrchu bez vnější smyčky, například kulového povrchu, je možné oříznutou oblast ovládat obrácením směru křivky smyčky.

Tolerance je tolerance použitá při rozhodování, zda jsou konce křivek totožné a zda jsou křivka a povrch totožné. Zadaná tolerance nemůže být menší než libovolná z tolerancí použitých při tvorbě vstupních objektů PolyCurve. Výchozí hodnota 0.0 znamená, že se použije největší tolerance použitá při tvorbě vstupních objektů PolyCurve.

V následujícím příkladu jsou dvě smyčky oříznuty z povrchu, čímž jsou vráceny dva nové povrchy zvýrazněné modře. Posuvník upravuje tvar nových povrchů.

___
## Vzorový soubor

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
