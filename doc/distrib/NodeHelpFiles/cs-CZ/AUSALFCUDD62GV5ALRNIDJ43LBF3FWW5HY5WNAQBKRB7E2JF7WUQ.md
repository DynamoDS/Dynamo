<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByCylinderPointsRadius --->
<!--- AUSALFCUDD62GV5ALRNIDJ43LBF3FWW5HY5WNAQBKRB7E2JF7WUQ --->
## In-Depth
V níže uvedeném příkladu je vytvořen povrch T-Spline základního válce. Dolní a horní rovina válce jsou definovány vstupy `startPoint` a `endPoint` a velikost je nastavena vstupní hodnotou `radius`. Rozpětí v radiálním a výškovém směru jsou řízena hodnotami `radiusSpans` a `heightSpans`. Počáteční symetrie tvaru je určena vstupem `symmetry'. Pokud je symetrie X nebo Y nastavena na hodnotu True, hodnota radiálních rozpětí musí být násobkem 4. Nakonec se k přepínání mezi režimem hladkého a kvádrového náhledu použije vstup `inSmoothMode`.

## Vzorový soubor

![Example](./AUSALFCUDD62GV5ALRNIDJ43LBF3FWW5HY5WNAQBKRB7E2JF7WUQ_img.jpg)
