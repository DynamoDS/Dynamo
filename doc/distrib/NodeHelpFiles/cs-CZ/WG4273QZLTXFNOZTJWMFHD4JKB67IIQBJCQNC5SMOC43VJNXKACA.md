<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConeCoordinateSystemHeightRadius --->
<!--- WG4273QZLTXFNOZTJWMFHD4JKB67IIQBJCQNC5SMOC43VJNXKACA --->
## In-Depth
V níže uvedeném příkladu je vytvořen kužel se základnou umístěnou na počátku souřadnicového systému, definovaného vstupem `cs`. Velikost kužele je definována vstupy `height` a `radius`. Rozpětí v radiálním a výškovém směru jsou řízena vstupy `radiusSpans` a `heightSpans`. Počáteční symetrie tvaru je určena vstupem `symmetry`. Pokud je symetrie X nebo Y nastavena na hodnotu True, hodnota radiálních rozpětí musí být násobkem 4. Nakonec vstup `inSmoothMode` umožňuje přepínání mezi režimem náhledu vyhlazení a kvádru povrchu T-Spline.

## Vzorový soubor

![Example](./WG4273QZLTXFNOZTJWMFHD4JKB67IIQBJCQNC5SMOC43VJNXKACA_img.jpg)
