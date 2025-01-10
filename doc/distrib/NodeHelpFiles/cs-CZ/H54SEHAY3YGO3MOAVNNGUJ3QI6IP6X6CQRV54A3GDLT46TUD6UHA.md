<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConePointsRadii --->
<!--- H54SEHAY3YGO3MOAVNNGUJ3QI6IP6X6CQRV54A3GDLT46TUD6UHA --->
## In-Depth
V níže uvedeném příkladu je vytvořen základní kužel T-Spline pomocí uzlu `TSplineSurface.ByConePointsRadii`. Polohu a výšku kužele určují dva vstupy – `startPoint` a `endPoint`. Poloměry základny a vrcholu je možné upravit pomocí vstupů `startRadius` a `topRadius`. Vstupy `radialSpans` a `heightSpans` určují radiální rozpětí a rozpětí výšky. Počáteční symetrie tvaru je určena vstupem `symmetry`. Pokud je symetrie X nebo Y nastavena na hodnotu True, hodnota radiálních rozpětí musí být násobkem 4. Nakonec vstup `inSmoothMode` umožňuje přepínání mezi režimem náhledu vyhlazení a kvádru u povrchu T-Spline.

## Vzorový soubor

![Example](./H54SEHAY3YGO3MOAVNNGUJ3QI6IP6X6CQRV54A3GDLT46TUD6UHA_img.jpg)
