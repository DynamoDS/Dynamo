<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
Uzel `TSplineSurface.ByPlaneOriginNormal` vygeneruje základní rovinný povrch T-Spline pomocí bodu počátku a normálového vektoru. K vytvoření roviny T-Spline používá uzel následující vstupy:
- `origin`: bod definující počátek roviny.
- `normal`: vektor určující směr normály vytvořené roviny.
- `minCorner` a `maxCorner`: rohy roviny reprezentované jako body s hodnotami X a Y (souřadnice Z budou ignorovány). Tyto rohy reprezentují rozsah výstupního povrchu T-Spline, pokud je přesunut na rovinu XY. Body `minCorner` a `maxCorner` se nemusí shodovat s vrcholy rohů ve 3D. Například pokud je vstup `minCorner` nastaven na hodnotu (0,0) a vstup `maxCorner` je nastaven na hodnotu (5,10), šířka a délka roviny bude 5 a 10.
- `xSpans` a `ySpans`: počet rozpětí šířky a délky / dělení roviny
- `symmetry`: určuje, zda je geometrie symetrická s ohledem na její osy X, Y a Z
- `inSmoothMode`: určuje, zda se výsledná geometrie zobrazí v režimu vyhlazení nebo kvádru

V níže uvedeném příkladu je rovinný povrch T-Spline vytvořen pomocí zadaného bodu počátku a normály, která je vektorem osy X. Velikost povrchu je řízena dvěma body, které se použijí jako vstupy `minCorner` a `maxCorner`.

## Vzorový soubor

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
