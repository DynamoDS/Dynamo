<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCoordinateSystemRadii --->
<!--- TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ --->
## In-Depth
V níže uvedeném příkladu je vytvořen povrch T-Spline anuloidu s počátkem v daném souřadnicovém systému ze vstupu `cs`. Vedlejší a hlavní poloměry tvaru jsou nastaveny vstupy `innerRadius` a `outerRadius`. Hodnoty vstupů `innerRadiusSpans` a `outerRadiusSpans` řídí definici povrchu ve dvou směrech. Počáteční symetrie tvaru je určena vstupem `symmetry`. Pokud je osová symetrie použitá na tento tvar aktivní na ose X nebo na ose Y, hodnota vstupu `outerRadiusSpans` anuloidu musí být násobkem 4. Radiální symetrie nemá žádný takový požadavek. Nakonec vstup `inSmoothMode` slouží k přepínání mezi režimy náhledu vyhlazení a kvádru u povrchu T-Spline.

## Vzorový soubor

![Example](./TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ_img.jpg)
