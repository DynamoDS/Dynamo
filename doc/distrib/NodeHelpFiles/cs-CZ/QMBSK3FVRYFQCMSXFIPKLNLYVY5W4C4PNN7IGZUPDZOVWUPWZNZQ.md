<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
Položka `TSplineSurface.ByPlaneBestFitThroughPoints` vygeneruje základní rovinu T-Spline pomocí seznamu bodů. K vytvoření roviny T-Spline používá uzel následující vstupy:
- `points`: sada bodů definujících orientaci roviny a počátek. V případech, kdy vstupní body neleží na jedné rovině, je orientace roviny určena podle optimálního proložení. K vytvoření povrchu jsou vyžadovány alespoň tři body.
- `minCorner` a `maxCorner`: rohy roviny reprezentované jako body s hodnotami X a Y (souřadnice Z budou ignorovány). Tyto rohy představují rozsah výstupního povrchu T-Spline, pokud je převeden na rovinu XY. Body `minCorner` a `maxCorner`se nemusí shodovat s vrcholy rohu ve 3D.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

V níže uvedeném příkladu je rovinný povrch T-Spline vytvořený náhodně vygenerovaným seznamem bodů. Velikost povrchu je řízena dvěma body, které se použijí jako vstupy `minCorner` a `maxCorner`.

## Vzorový soubor

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
