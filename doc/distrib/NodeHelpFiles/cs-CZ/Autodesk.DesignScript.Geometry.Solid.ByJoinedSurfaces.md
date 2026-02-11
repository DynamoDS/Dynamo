## Podrobnosti
Uzel Solid by Joined Surfaces přijímá jako vstup seznam povrchů a vrací jedno těleso definované povrchy. Povrchy musí definovat uzavřený povrch. V níže uvedeném příkladu začneme s kružnicí jako základní geometrií. Kružnice je záplatovaná, aby se vytvořil povrch a tento povrch bude přesunut ve směru Z. Poté vysuneme kružnici tak, abychom vytvořili strany. Pomocí metody List.Create vytvoříme seznam skládající se ze základních, bočních a horních povrchů a poté pomocí uzlu ByJoinedSurfaces proměníme seznam v jedno uzavřené těleso.
___
## Vzorový soubor

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

