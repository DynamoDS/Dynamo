## In-Depth
V níže uvedeném příkladu je vytvořen základní kužel T-Spline pomocí uzlu `TSplineSurface.ByConePointsRadius`. Polohu a výšku kužele určují dva vstupy hodnot `startPoint` a `endPoint`. Upravit je možné pouze základní poloměr pomocí vstupu `radius` a horní poloměr je vždy nulový. Vstupy `radialSpans` a `heightSpans` určují radiální rozpětí a rozpětí výšky. Počáteční symetrii tvaru určuje vstup `symmetry`. Pokud je symetrie X nebo Y nastavena na hodnotu True, hodnota radiálních rozpětí musí být násobek 4. Nakonec vstup `inSmoothMode` umožňuje přepínat mezi režimem náhledu vyhlazení nebo kvádru u povrchu T-Spline.

## Vzorový soubor

![Example](./GVO3NNSNHNAH3DJS5OR37DI2A457QGYX4BQGMHO4IGUUUHZV3HSQ_img.jpg)
