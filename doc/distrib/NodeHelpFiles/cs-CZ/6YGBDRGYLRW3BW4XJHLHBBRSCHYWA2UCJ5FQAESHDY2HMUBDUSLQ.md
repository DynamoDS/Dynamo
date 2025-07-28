<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
Příkaz `TSplineSurface.AddReflections` vytvoří nový povrch T-Spline použitím jednoho nebo více odrazů na vstup `tSplineSurface`. Booleovský vstup `weldSymmetricPortions` určuje, zda jsou vyostřené hrany generované odrazem vyhlazeny nebo zachovány.

V níže uvedeném příkladu je znázorněno, jak je možné přidat více odrazů k povrchu T-Spline pomocí uzlu `TSplineSurface.AddReflections`. Vytvoří se dva odrazy – osový a radiální. Základní geometrie je povrch T-spline ve tvaru tažení s trajektorií oblouku. Dva odrazy jsou spojeny v seznamu a použijí se jako vstup pro uzel `TSplineSurface.AddReflections` společně se základní geometrií, od které chcete vytvořit odraz. Objekty TSplineSurfaces jsou svařeny, což vede k vyhlazenému objektu TSplineSurface bez ostrých hran. Povrch je dále upraven přesunutím jednoho vrcholu pomocí uzlu `TSplineSurface.MoveVertex`. Kvůli tomu, že se odraz použije na povrch T-Spline, pohyb vrcholu je proveden 16krát.

## Vzorový soubor

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
