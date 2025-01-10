<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCenterRadii --->
<!--- TAX4CBLVHC7JXO4PNOKK44X5VVCC377TK4Q3R5UBTYQROUPG4VCQ --->
## In-Depth
V níže uvedeném příkladu je vytvořen povrch T-Spline anuloidu kolem daného středu ze vstupu `center`. Vedlejší a hlavní poloměry tvaru jsou nastaveny vstupy `innerRadius` a `outerRadius`. Hodnoty vstupů `innerRadiusSpans` a `outerRadiusSpans` řídí definici povrchu ve dvou směrech. Počáteční symetrie tvaru je určena vstupem `symmetry`. Pokud je osová symetrie použitá na tento tvar aktivní na ose X nebo na ose Y, hodnota vstupu `outerRadiusSpans` anuloidu musí být násobkem 4. Radiální symetrie nemá žádný takový požadavek. Nakonec vstup `inSmoothMode` slouží k přepínání mezi režimy náhledu vyhlazení a kvádru u povrchu T-Spline.

## Vzorový soubor

![Example](./TAX4CBLVHC7JXO4PNOKK44X5VVCC377TK4Q3R5UBTYQROUPG4VCQ_img.jpg)


